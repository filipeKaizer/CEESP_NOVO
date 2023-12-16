using System.Collections.Generic;

namespace CEESP
{
    public static class ListData1
    {
        public static List<ColectedData> colectedData { get; set; } = new List<ColectedData>();
        public static List<ColectedData> cache { get; set; } = new List<ColectedData>();

        public static ConfigData configData { get; set; } = new ConfigData();
    }
}
