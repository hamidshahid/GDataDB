using System;
using System.IO;
using GDataDB.Impl;
using Google.GData.Client;
using Google.GData.Documents;
using Google.GData.Spreadsheets;
using SpreadsheetQuery=Google.GData.Documents.SpreadsheetQuery;
using System.Text;

namespace GDataDB {
	public class DatabaseClient : IDatabaseClient {
        	private readonly IService documentService;
        	private readonly IService spreadsheetService;

        	public IService DocumentService
        	{
            		get { return documentService; }
        	}

        	public IService SpreadsheetService
        	{
            		get { return spreadsheetService; }
        	}

		public DatabaseClient(string username, string password) {
			var docService = new DocumentsService("database");
			docService.setUserCredentials(username, password);
			documentService = docService;

			var ssService = new SpreadsheetsService("database");
			ssService.setUserCredentials(username, password);
			spreadsheetService = ssService;
		}

		public IDatabase CreateDatabase(string name) {
            /*
            // GDataRequestException : Can not update a read-only feed
            var entry = new DocumentEntry();
            entry.Title.Text = name;
            entry.Categories.Add(DocumentEntry.SPREADSHEET_CATEGORY);
            var feed = new AtomFeed(new Uri(DocumentsListQuery.documentsBaseUri), DocumentService);
            var newEntry = DocumentService.Insert(feed, entry);
            return new Database(this, newEntry);
             */

            using (var ms = new MemoryStream()) {
				using (var sw = new StreamWriter(ms)) {
					sw.WriteLine(",,,");
                    sw.Flush();
                    ms.Position = 0;
                    var spreadSheet = DocumentService.Insert(new Uri(DocumentsListQuery.documentsBaseUri + "?convert=true"), ms, "text/csv", name);
                    var db = new Database(this, spreadSheet);

                    // Get default table and rename it to something random to make it "invisible".
                    // Otherwise Google throws "Blank rows cannot be written"
                    var t = db.GetTable<object>(name);
                    t.Rename(Guid.NewGuid().ToString());
                    return db;
                }
            }
        }

		public IDatabase GetDatabase(string name) {
			var feed = DocumentService.Query(new SpreadsheetQuery {TitleExact = true, Title = name });
			if (feed.Entries.Count == 0)
				return null;
			return new Database(this, feed.Entries[0]);
		}
	}
}