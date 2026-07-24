using CsvHelper;
using CsvHelper.Configuration;
using ERS.Core.Interfaces;
using ERS.Core.Model;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ERS.Core.DataPipeline.Udemy
{
    public class UdemyCsvExtractor : ICourseExtractor
    {
        private readonly string filePath;

        public string ProviderName => "Udemy CSV";

        public UdemyCsvExtractor(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            this.filePath = filePath;
        }

        public async IAsyncEnumerable<Course> ExtractAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                BadDataFound = null
            };

            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, config);

            var asyncRecords = csv.GetRecordsAsync<UdemyCsvRecord>(cancellationToken);

            await foreach (var record in asyncRecords)
            {
                var course = UdemyCourseAdapter.Adapt(record);

                if (course != null && !string.IsNullOrWhiteSpace(course.Description))
                {
                    yield return course;
                }
            }
        }
    }
}