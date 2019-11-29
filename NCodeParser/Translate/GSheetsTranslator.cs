using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using NCodeParser.Interface;
using static Google.Apis.Sheets.v4.SpreadsheetsResource.ValuesResource.UpdateRequest;

namespace NCodeParser.Translate
{
	public class GSheetsTranslator : ITranslator
	{
		private readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
		private readonly string SheetID = "1JZwDTknNzCIpFZNoDLqaWEZcm5oJYUiyD5kPH9T7VAI";

		private readonly bool[] RowUsages = new bool[10];
		private readonly SemaphoreSlim MainSemaphore = new SemaphoreSlim(10, 10);
		private readonly SemaphoreSlim IDSemaphore = new SemaphoreSlim(1, 1);

		public async Task<string> Translate(string input)
		{
			int rowID = -1;

			try
			{
				await MainSemaphore.WaitAsync();

				UserCredential credential;

				if (!File.Exists("credentials.json"))
				{
					return input;
				}

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
						new FileDataStore(credPath, true));
				}

				var sheetService = new SheetsService(new BaseClientService.Initializer()
				{
					HttpClientInitializer = credential,
					ApplicationName = Config.ApplicationName,
				});

				rowID = await GetRowID();
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

				var request = sheetService.Spreadsheets.Values.Update(range, SheetID, $"{from}:{to}");
				request.ValueInputOption = ValueInputOptionEnum.USERENTERED;

				var response = await request.ExecuteAsync();

				var result = await Load(rowID);
				if (string.IsNullOrWhiteSpace(result))
				{
					return input;
				}
				else
				{
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
				await IDSemaphore.WaitAsync();

				while (true)
				{
					for (int i = 0; i < RowUsages.Length; i++)
					{
						if (!RowUsages[i])
						{
							Console.WriteLine(i + 1);

							RowUsages[i] = true;

							return i + 1;
						}
					}

					await Task.Delay(500);
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
						new FileDataStore(credPath, true));
				}

				var sheetService = new SheetsService(new BaseClientService.Initializer()
				{
					HttpClientInitializer = credential,
					ApplicationName = Config.ApplicationName,
				});

				string from = "A" + rowID;
				string to = "E" + rowID;

				var request = sheetService.Spreadsheets.Values.BatchGet(SheetID);
				request.Ranges = $"{from}:{to}";

				var response = await request.ExecuteAsync();

				return response.ValueRanges[0].Values[0][1].ToString();
			}
			catch
			{

			}

			return "";
		}
	}
}
