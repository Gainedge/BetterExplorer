using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterExplorer
{
    public class ConditionalSelectParameters
    {

        public class FileNameParameters
        {

            public string query = "";
            public FileNameFilterTypes filter = FileNameFilterTypes.Contains;
            public bool matchCase = false;

            public FileNameParameters(string querystr, FileNameFilterTypes filterType, bool SearchmatchCase)
            {
                this.filter = filterType;
                this.query = querystr;
                this.matchCase = SearchmatchCase;
            }
            
        }

        public enum FileNameFilterTypes
        {
            Contains = 0,
            StartsWith = 1,
            EndsWith = 2,
            Equals = 3,
            DoesNotContain = 4,
            NotEqualTo = 5,
        }

        public class FileSizeParameters
        {

            public long query1 = 0;
            public long query2 = 0;
            public FileSizeFilterTypes filter = FileSizeFilterTypes.LargerThan;

            public FileSizeParameters(long q1, long q2, FileSizeFilterTypes filterType)
            {
                query1 = q1;
                query2 = q2;
                filter = filterType;
            }

        }

        public enum FileSizeFilterTypes
        {
            LargerThan = 0,
            SmallerThan = 1,
            Equals = 2,
            Between = 3,
            NotEqualTo = 4,
            NotBetween = 5,
        }

        public class DateParameters
        {

            public DateTime queryDate = DateTime.Now.Date;
            public DateFilterTypes filter = DateFilterTypes.EarlierThan;

            public DateParameters(DateTime query, DateFilterTypes filterType)
            {
                this.queryDate = query.Date;
                this.filter = filterType;
            }

        }

        public enum DateFilterTypes
        {
            EarlierThan = 0,
            LaterThan = 1,
            Equals = 2,
        }

    }

    public class ConditionalSelectData
    {
        public ConditionalSelectParameters.FileNameParameters FileNameData;
        public ConditionalSelectParameters.FileSizeParameters FileSizeData;
        public ConditionalSelectParameters.DateParameters DateCreatedData;
        public ConditionalSelectParameters.DateParameters DateModifiedData;
        public ConditionalSelectParameters.DateParameters DateAccessedData;

        public bool FilterByFileName = true;
        public bool FilterByFileSize = true;
        public bool FilterByDateCreated = true;
        public bool FilterByDateModified = true;
        public bool FilterByDateAccessed = true;

        public ConditionalSelectData(ConditionalSelectParameters.FileNameParameters filedata, ConditionalSelectParameters.FileSizeParameters sizedata, ConditionalSelectParameters.DateParameters createddata, ConditionalSelectParameters.DateParameters modifieddata, ConditionalSelectParameters.DateParameters accessedata, bool usefilename, bool usefilesize, bool usecreated, bool usemodified, bool useaccessed)
        {
            this.FileNameData = filedata;
            this.FileSizeData = sizedata;
            this.DateCreatedData = createddata;
            this.DateModifiedData = modifieddata;
            this.DateAccessedData = accessedata;
            this.FilterByDateAccessed = useaccessed;
            this.FilterByDateCreated = usecreated;
            this.FilterByDateModified = usemodified;
            this.FilterByFileName = usefilename;
            this.FilterByFileSize = usefilesize;
        }

    }
}
