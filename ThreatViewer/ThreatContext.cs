using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
//using System.Runtime.InteropServices;

namespace ThreatViewer
{
    class ThreatContext : DbContext
    {
        public static string DateBaseFile = App.DataDirectory + @"\ThreatContext.mdf";
        private readonly string XlsxFileUrl = "https://bdu.fstec.ru/documents/files/thrlist.xlsx";
        private readonly string TempXmlFilePath = App.DataDirectory + @"\update.xlsx";

        public DbSet<Threat> Threats { get; set; }

        public ThreatContext() : base("ThreatContext") { }

        public Task<Queue<Change>> TryUpdate()
        {
            try
            {
                return Task.FromResult(Update());
            }
            catch (Exception e)
            {
                return Task.FromException<Queue<Change>>(e);
            }
        }

        public Queue<Change> Update()
        {
            var changes = new Queue<Change>();
            new WebClient().DownloadFile(XlsxFileUrl, TempXmlFilePath);

            using (var excel = new ExcelReader(TempXmlFilePath))
            {
                excel.OpenTable(0);
                int rows = excel.GetRowsCount();

                var allUpdatedID = new List<int>();
                Threat databaseThreat, updatedThreat;
                for (int i = 2; i < rows; i++)
                {
                    updatedThreat = new Threat()
                    {
                        Number = (int)excel.TryRead<double>(i, 0),
                        Name = excel.TryRead<string>(i, 1),
                        Discription = excel.TryRead<string>(i, 2),
                        Source = excel.TryRead<string>(i, 3),
                        Object = excel.TryRead<string>(i, 4),
                        IsPrivacyViolation = (int)excel.TryRead<double>(i, 5) == 1 ? true : false,
                        IsIntegrityViolation = (int)excel.TryRead<double>(i, 6) == 1 ? true : false,
                        IsAccessibilityViolation = (int)excel.TryRead<double>(i, 7) == 1 ? true : false,
                        LastUpdateTime = excel.TryRead<DateTime>(i, 9).ToBinary()
                    };

                    allUpdatedID.Add(updatedThreat.Number);

                    databaseThreat = Threats.FirstOrDefault(x => x.Number == updatedThreat.Number);
                    // Если нашло в базе строчку
                    if (databaseThreat != null)
                    {
                        // Если строчка такая же
                        if (databaseThreat.LastUpdateTime == updatedThreat.LastUpdateTime)
                            continue;
                        // Если строчка изменена
                        else
                        {
                            changes.Enqueue(new Change(databaseThreat.Clone(), updatedThreat));
                            updatedThreat.ID = databaseThreat.ID;
                            Entry(databaseThreat).CurrentValues.SetValues(updatedThreat);
                            continue;
                        }
                    }
                    // Если такой строчки не было
                    else
                    {
                        changes.Enqueue(new Change(null, updatedThreat));
                        Threats.Add(updatedThreat);
                        continue;
                    }
                }

                // Если строчка удалена в новой базе
                foreach (var item in Threats.Where(x => !allUpdatedID.Contains(x.Number)))
                {
                    Entry(item).State = EntityState.Deleted;
                    changes.Enqueue(new Change(item, null));
                }
            }

            SaveChanges();

            File.Delete(TempXmlFilePath);

            return changes;
            
        }
    }
}
