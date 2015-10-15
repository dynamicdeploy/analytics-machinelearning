/*  
Copyright (c) Microsoft.  All rights reserved.  Licensed under the MIT License.  See LICENSE in the root of the repository for license information 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnomalyClientLib
{
    public class StringTable
    {
        public string[] ColumnNames { get; set; }
        public string[,] Values { get; set; }
    }
    public class Rootobject
    {
        public Results Results { get; set; }
    }

    public class Results
    {
        public Output1 output1 { get; set; }
        public Output2 output2 { get; set; }
    }

    public class Output1
    {
        public string type { get; set; }
        public Value value { get; set; }
    }

    public class Value
    {
        public string[] ColumnNames { get; set; }
        public string[] ColumnTypes { get; set; }
        public string[][] Values { get; set; }
    }

    public class Output2
    {
        public string type { get; set; }
        public Value1 value { get; set; }
    }

    public class Value1
    {
        public string[] ColumnNames { get; set; }
        public string[] ColumnTypes { get; set; }
        public string[][] Values { get; set; }
    }

}
