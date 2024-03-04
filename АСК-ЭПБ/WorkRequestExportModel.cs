using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace АСК_ЭПБ
{
    public class WorkRequestExportModel
    {
        public int WORK_REQUESTID { get; set; }
        public DateTime DATA_CREATION { get; set; }
        public string URL_WORK_REQUEST { get; set; }
        public string NAME_EQUIPMENT { get; set; }
        public string NAME_WORK_TYPE { get; set; }
        public string NAME_REQUEST_STATUS { get; set; }
        public string NAME_CONTRACTOR { get; set; }
    }
}
