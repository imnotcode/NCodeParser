using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using NCodeParser.Utility;
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource.UpdateRequest;

namespace NCodeParser.Translate
{
	public class GSheetsTranslator : Translator, IDisposable
	{
		private readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
		private readonly string SheetID = "1JZwDTknNzCIpFZNoDLqaWEZcm5oJYUiyD5kPH9T7VAI";

		private readonly bool[] RowUsages = new bool[3];
		private readonly SemaphoreSlim MainSemaphore = new SemaphoreSlim(3, 3);
		private readonly SemaphoreSlim IDSemaphore = new SemaphoreSlim(1, 1);

		private SheetsService Service;

		public GSheetsTranslator()
		{

		}

		public async Task InitializeService()
		{
			try
			{
				if (!File.Exists("credentials.json"))
				{
					return;
				}

				UserCredential credential;

				using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
				{
					// The file token.json stores the user's access and refresh tokens, and is created
					// automatically when the authorization flow completes for the first time.
					string credPath = "token.json";
					credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
						GoogleClientSecrets.Load(stream).Secrets,
						Scopes,
						"user",
						CancellationToken.None,
						new FileDataStore(credPath, true)).ConfigureAwait(false);
				}

				Service = new SheetsService(new BaseClientService.Initializer()
				{
					HttpClientInitializer = credential,
					ApplicationName = Config.ApplicationName
				});
			}
			catch
			{

			}
		}

		public override async Task<string> Translate(string input)
		{
			var dividedTexts = StringUtil.DivideString(input);

			var sourceList = new List<string>();
			var builder = new StringBuilder();

			for (int i = 0; i < dividedTexts.Length; i++)
			{
				if (!string.IsNullOrWhiteSpace(dividedTexts[i]))
				{
					sourceList.Add(dividedTexts[i]);

					builder.Append(dividedTexts[i]);
					builder.Append(" | ");
				}
			}

			var result = await TranslateOneLine(builder.ToString()).ConfigureAwait(false);
			var splitTexts = result.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);

			if (sourceList.Count != splitTexts.Length)
			{

			}

			builder.Clear();
			for (int i = 0, j = 0; i < dividedTexts.Length; i++)
			{
				if (!string.IsNullOrWhiteSpace(dividedTexts[i]))
				{
					var translatedText = splitTexts[j++].Trim();

					if (Config.TranslateWithSource && dividedTexts[i] != translatedText)
					{
						builder.Append(dividedTexts[i]);
						builder.Append(Environment.NewLine);
					}

					builder.Append(translatedText);
				}

				builder.Append(Environment.NewLine);
			}

			return builder.ToString();
		}

		protected override async Task<string> TranslateOneLine(string input)
		{
			int rowID = -1;

			try
			{
				if (Service == null)
				{
					return input;
				}

				await MainSemaphore.WaitAsync().ConfigureAwait(false);

				rowID = await GetRowID().ConfigureAwait(false);
				if (rowID == -1)
				{
					return input;
				}

				string from = "A" + rowID;
				string to = "E" + rowID;

				IList<IList<object>> list = new List<IList<object>>();
				list.Add(new List<object>() { input, "=GOOGLETRANSLATE(" + from + ", \"ja\", \"ko\")" });

				var range = new ValueRange();
				range.Values = list;

				var request = Service.Spreadsheets.Values.Update(range, SheetID, $"{from}:{to}");
				request.ValueInputOption = ValueInputOptionEnum.USERENTERED;

				var response = await request.ExecuteAsync().ConfigureAwait(false);

				var result = await Load(rowID).ConfigureAwait(false);
				if (string.IsNullOrWhiteSpace(result))
				{
					return input;
				}
				else
				{
					if (result == "로드 중...")
					{
						ReleaseRowID(rowID);

						return await TranslateOneLine(input).ConfigureAwait(false);
					}

					return result;
				}
			}
			catch
			{

			}
			finally
			{
				if (rowID != -1)
				{
					ReleaseRowID(rowID);
				}

				MainSemaphore.Release();
			}

			return input;
		}

		private async Task<int> GetRowID()
		{
			try
			{
				await IDSemaphore.WaitAsync().ConfigureAwait(false);

				while (true)
				{
					for (int i = 0; i < RowUsages.Length; i++)
					{
						if (!RowUsages[i])
						{
							RowUsages[i] = true;

							return i + 1;
						}
					}

					await Task.Delay(500).ConfigureAwait(false);
				}
			}
			catch
			{

			}
			finally
			{
				IDSemaphore.Release();
			}

			return -1;
		}

		private void ReleaseRowID(int rowID)
		{
			RowUsages[rowID - 1] = false;
		}

		private async Task<string> Load(int rowID)
		{
			try
			{
				if (Service == null)
				{
					return "";
				}

				string from = "A" + rowID;
				string to = "E" + rowID;

				var request = Service.Spreadsheets.Values.BatchGet(SheetID);
				request.Ranges = $"{from}:{to}";

				var response = await request.ExecuteAsync().ConfigureAwait(false);

				return response.ValueRanges[0].Values[0][1].ToString();
			}
			catch
			{

			}

			return "";
		}

		public void Dispose()
		{
			// TODO
		}
	}
}
