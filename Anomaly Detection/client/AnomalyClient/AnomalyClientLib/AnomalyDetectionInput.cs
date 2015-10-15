/*  
Copyright (c) Microsoft.  All rights reserved.  Licensed under the MIT License.  See LICENSE in the root of the repository for license information 
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace AnomalyClientLib
{
   

    public static class Direction
    {
        public const string POS = "pos";
        public const string NEG = "neg";
        public const string BOTH = "both";
    }
   
    public static class OnlyLast
    {
        public const string NONE = "None";
        public const string DAY = "day";
        public const string HR = "hr";
    }
   

    public static class Threshold
    {
        public const string NONE = "None";
        public const string MED_MAX = "day";
        public const string P95 = "p95";
        public const string P99 = "p99";
    }

    public class AnomalyDetectionInput
    {
        private double _maxAnomaliesToReturn = 0.1;
        
        private string _direction = "both";

        private double _alpha = 0.05;

        private string _onlyLast = "None";

        private string _threshold = "None";

        private bool _addExpectedValueColumn = false;

        private bool? _isLongtermTimeSeries = null;

        private int _piecewiseMedianTimeWindowInWeeks = 2;

        private bool _createPlot = false;

        private bool? _applyLogScaling = null;

        private string _xAxisLabel = "X";
        private string _yAxisLabel = "Y";

        private string _titleForPlot = "Anomalies";

        private bool _removeNAs = false;

        private string _inputDataFileName = null;

        private string _outputFileName = null;

        public double MaxAnomaliesToReturn
        {
            get
            {
                return _maxAnomaliesToReturn;
            }

            set
            {
                _maxAnomaliesToReturn = value;
            }
        }

        public string Direction
        {
            get
            {
                return _direction;
            }

            set
            {
                _direction = value;
            }
        }

        public double Alpha
        {
            get
            {
                return _alpha;
            }

            set
            {
                _alpha = value;
            }
        }

        public string OnlyLast
        {
            get
            {
                return _onlyLast;
            }

            set
            {
                _onlyLast = value;
            }
        }

        public string Threshold
        {
            get
            {
                return _threshold;
            }

            set
            {
                _threshold = value;
            }
        }

        public bool AddExpectedValueColumn
        {
            get
            {
                return _addExpectedValueColumn;
            }

            set
            {
                _addExpectedValueColumn = value;
            }
        }

        public int PiecewiseMedianTimeWindowInWeeks
        {
            get
            {
                return _piecewiseMedianTimeWindowInWeeks;
            }

            set
            {
                _piecewiseMedianTimeWindowInWeeks = value;
            }
        }

        public bool CreatePlot
        {
            get
            {
                return _createPlot;
            }

            set
            {
                _createPlot = value;
            }
        }

        public bool? ApplyLogScaling
        {
            get
            {
                return _applyLogScaling;
            }

            set
            {
                _applyLogScaling = value;
            }
        }

        public string XAxisLabel
        {
            get
            {
                return _xAxisLabel;
            }

            set
            {
                _xAxisLabel = value;
            }
        }

        public string YAxisLabel
        {
            get
            {
                return _yAxisLabel;
            }

            set
            {
                _yAxisLabel = value;
            }
        }

        public string TitleForPlot
        {
            get
            {
                return _titleForPlot;
            }

            set
            {
                _titleForPlot = value;
            }
        }

        public bool RemoveNAs
        {
            get
            {
                return _removeNAs;
            }

            set
            {
                _removeNAs = value;
            }
        }

        public bool? IsLongtermTimeSeries
        {
            get
            {
                return _isLongtermTimeSeries;
            }

            set
            {
                _isLongtermTimeSeries = value;
            }
        }

        public string InputDataFileName
        {
            get
            {
                return _inputDataFileName;
            }

            set
            {
                _inputDataFileName = value;
            }
        }

        public string OutputFileName
        {
            get
            {
                return _outputFileName;
            }

            set
            {
                _outputFileName = value;
            }
        }
    }

    public class StringValue : System.Attribute
    {
        private string _value;

        public StringValue(string value)
        {
            _value = value;
        }

        public string Value
        {
            get { return _value; }
        }

    }

    /// <summary>
    /// string valueOfAuthenticationMethod = StringEnum.GetStringValue(AuthenticationMethod.FORMS);
    /// </summary>
    public static class StringEnum
    {
        public static string GetStringValue(Enum value)
        {
            string output = null;
            Type type = value.GetType();

            //Check first in our cached results...

            //Look for our 'StringValueAttribute' 

            //in the field's custom attributes

            FieldInfo fi = type.GetField(value.ToString());
            StringValue[] attrs =
               fi.GetCustomAttributes(typeof(StringValue),
                                       false) as StringValue[];
            if (attrs.Length > 0)
            {
                output = attrs[0].Value;
            }

            return output;
        }
    }
}
