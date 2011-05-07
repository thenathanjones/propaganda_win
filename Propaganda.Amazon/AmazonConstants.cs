using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Propaganda.AmazonService
{
    public class AmazonConstants
    {
        /// <summary>
        /// Web services ID to be used with Amazon
        /// </summary>
        public const string AMAZON_AWS_KEY_ID = "0FW7P5S0JWJKXYAVF8R2";

        /// <summary>
        /// Threshold for string matching Amazon entries
        /// </summary>
        public const int LEVENSHTEIN_THRESHOLD = 10;
    }
}
