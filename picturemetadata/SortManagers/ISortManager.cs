using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace picturemetadata
{
    internal interface ISortManager
    {
        List<string> sortPictures(DirectoryInfo dirInfo);
    }
}
