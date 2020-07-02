using System;
using System.Collections.Generic;
using com.bitscopic.hilleman.core.utils;

namespace com.bitscopic.hilleman.core.domain
{
    [Serializable]
    public class PersonName
    {
        public String nameString;
        public String last;
        public String first;
        public String middle;
        public String maiden;
        public String title;
        /// <summary>
        /// e.g. PhD
        /// e.g. PhD, MD
        /// </summary>
        public IList<String> suffixes;

        public PersonName() { }

        /// <summary>
        /// DUCK,DON
        /// POOH,WINNY THE
        /// HILLEMAN,MAURICE R MD
        /// </summary>
        /// <param name="nameString"></param>
        public PersonName(String nameString)
        {
            this.nameString = nameString;

            if (!String.IsNullOrEmpty(nameString) && nameString.Contains(","))
            {
                String[] pieces = StringUtils.split(nameString, StringUtils.COMMA);
                this.last = pieces[0];
                if (pieces[1].Contains(" ")) // looks like there's a middle
                {
                    String[] firstMiddleSuffixPieces = StringUtils.split(pieces[1], " ");
                    this.first = firstMiddleSuffixPieces[0];
                    this.middle = firstMiddleSuffixPieces[1];
                    if (firstMiddleSuffixPieces.Length > 2)
                    {
                        //this.middle = p
                    }
                }
                else
                {
                    this.first = pieces[1];
                }
            }
            
        }
    }
}