using System;
using System.Collections.Generic;
using System.Text;

namespace WeahetBot
{
    public static class JSON_API_Decryptor
    {
        public const string R_TITLE = "r/title";
        public const string R_TITLEIN = "r/titleIn";
        public const string R_DESCR = "r/descr";
        public const string R_LAT = "r/lat";
        public const string R_LON = "r/lon";
        public const string R_ID = "r/id";

        public const string TABS_X_MIN = "tabs/[x]/min";
        public const string TABS_X_MAX = "tabs/[x]/max";

        public const string DAYS_X_SR = "days/[x]/sr";
        public const string DAYS_X_SS = "days/[x]/ss";
        public const string DAYS_X_TF = "days/[x]/tf";
        public const string DAYS_X_NF = "days/[x]/nf";

        public const string DAYS_X_SW_X_TXT = "days/[x]/sw/[0]/txt";
        public const string DAYS_X_SW_X_DESCR = "days/[x]/sw/[0]/descr";

        public const string DAYS_X_HOURS_X_H = "days/[x]/sw/[x]/h";
        public const string DAYS_X_HOURS_X_T = "days/[x]/sw/[x]/t";
        public const string DAYS_X_HOURS_X_TF = "days/[x]/sw/[x]/tf";
        public const string DAYS_X_HOURS_X_P = "days/[x]/sw/[x]/p";
        public const string DAYS_X_HOURS_X_W = "days/[x]/sw/[x]/w";
        public const string DAYS_X_HOURS_X_WD = "days/[x]/sw/[x]/wd";
        public const string DAYS_X_HOURS_X_WS = "days/[x]/sw/[x]/ws";
        
        public const string TN = "tn";
        

        public static void Load_JSON_String(string jsonFile)
        {
            //string str = ExtractSubField(ExtractField(jsonFile, "r"), "lat");
            //string str = ExtractSubField(ExtractField(jsonFile, "r"), "title ");
            //string str = ExtractField(jsonFile, "days");
            //str = ExtractSubField(str, "tf");
            //str = ConstructString_FromUnicode(str);
            //string str = ExtractIndexedSubField(jsonFile, 6);
            //str = (ExtractSubField(str, "max"));
            //Console.WriteLine("\n\n\nS\n" + GetIndexedSubFieldCount(jsonFile));
            //string str = ExtractField(jsonFile, "tabs");

            //string str = ExtractSubField(ExtractField(jsonFile, "r"), "id");
            //str = ConstructString_FromUnicode(str);

            //string str = ExtractIndexedSubField(jsonFile, 2);
            //string str = ExtractIndexedField(jsonFile, "days");
            //str = ExtractIndexedField(str, "sw");
            //str = ExtractIndexedSubField(str, 0);
            //str = ExtractSubField(str, "txt");
            //str = ConstructString_FromUnicode(str);
            //str = ExtractSubField(str, "min");

            //string str = GetStringAtPath(jsonFile, "days/[2]/ss");

            string str = ExtractIndexedField(jsonFile, "days");
            str = ExtractIndexedSubField(str, 1);

            Console.WriteLine(str);
            //Console.WriteLine("\n\n" + ExtractSubField(jsonFile, "tn"));
            //if (str.Contains("\\u"))
            //{
            //    Console.WriteLine("\n" + ConstructString_FromUnicode(str) + "\n\n\n");
            //}
            //else
            //{
            //    Console.WriteLine(str + "\n\n\n");
            //}

            //Console.WriteLine((title) + "\n\n\n");
            //Console.WriteLine(ExtractField(jsonFile, "r") + "\n\n");
            //Console.WriteLine(ConstructString_FromUnicode(title) + "\n\n\n");
            //Console.WriteLine(str + "\n\n\n");
            //Console.WriteLine(jsonFile);
        }


        public static string GetStringAtPath(string sourceString, string jsonPath)
        {
            string[] tokens = jsonPath.Split('/');

            bool isBegin = false;
            bool isIndexedField = false;
            //int fieldIndex = 0;
            

            for (int i = 0; i < tokens.Length; i++)
            {
                if(isBegin)
                {
                    if (isIndexedField)
                    {
                        isIndexedField = false;

                        string indexStr = tokens[i].Split('[')[1];
                        indexStr = indexStr.Split(']')[0];
                        //Console.WriteLine(sourceString);
                        sourceString = ExtractIndexedSubField(sourceString, Convert.ToInt32(indexStr));
                        continue;
                    }
                    else
                    {
                        if (IsIndexedField(sourceString, tokens[i]))
                        {
                            isIndexedField = true;
                        }
                        else
                        {
                            sourceString = ExtractSubField(sourceString, tokens[i]);
                        }
                    }
                }
                else
                {
                    if (IsIndexedField(sourceString, tokens[i]))
                    {
                        sourceString = ExtractIndexedField(sourceString, tokens[i]);
                        isIndexedField = true;
                    }
                    else
                    {
                        sourceString = ExtractField(sourceString, tokens[i]);
                    }
                }

                if (isBegin == false)
                {
                    isBegin = true;
                }
            }

            if (tokens.Length == 1)
            {
                sourceString = ExtractSubField(sourceString, tokens[0]);
            }

            if (sourceString.Contains("\\u"))
            {
                sourceString = ConstructString_FromUnicode(sourceString);
            }
            //foreach (var item in tokens)
            //{
            //    Console.WriteLine(item);
            //}
            return sourceString;
        }

        public static bool IsIndexedField(string sourceString, string fieldName)
        {
            bool isSquareBracketFound = false;
            //Console.WriteLine(sourceString.IndexOf("\"" + fieldName + "\""));
            //Console.WriteLine("\"" + fieldName + "\"");
            //Console.WriteLine(sourceString);
            sourceString = sourceString.Substring(sourceString.IndexOf("\"" + fieldName + "\""));
            for (int i = 0; i < sourceString.Length; i++)
            {
                if (sourceString[i] == '{')
                {
                    if (isSquareBracketFound)
                        return true;
                    else return false;
                }
                else if (sourceString[i] == '[')
                {
                    isSquareBracketFound = true;
                }
            }
            return false;
        }

        public static string ExtractField(string sourceString, string fieldName)
        {
            int index = sourceString.IndexOf("\"" + fieldName + "\"");
            if (index == -1)
            {
                return "ERROR_NO_FIELD";
            }
            string splittedStr = sourceString.Substring(index);
            int splitIndex = 0;
            int openBraces = 0;
            bool closedBraceFound = false;
            for (int i = 0; i < splittedStr.Length; i++)
            {
                if (splittedStr[i] == '{')
                {
                    openBraces++;
                }
                else if (splittedStr[i] == '}')
                {
                    closedBraceFound = true;
                    openBraces--;
                }

                if (openBraces < 1 && closedBraceFound)
                {
                    splitIndex = i;
                    break;
                }
            }

            splittedStr = splittedStr.Substring(0, splitIndex);
            
            return splittedStr + "}";
        }

        public static string ExtractSubField(string sourceString, string subFieldName)
        {
            bool isString = false;
            int index = sourceString.IndexOf("\"" + subFieldName + "\"");
            if (index == -1)
            {
                return "ERROR_NO_SUB_FIELD";
            }
            string splittedStr = sourceString.Substring(index + subFieldName.Length + 3);

            bool isNumber = false;
            for (int i = 0; i < splittedStr.Length; i++)
            {
                if (splittedStr[i] != ' ')
                {
                    if (splittedStr[i] == '\"')
                    {
                        isNumber = false;
                        break;
                    }
                    else if (splittedStr[i] == ',')
                    {
                        isNumber = true;
                        break;
                    }
                }
            }

            if (isNumber)
            {
                splittedStr = splittedStr.Substring(0, splittedStr.IndexOf(','));
            }
            else
            {
                splittedStr = splittedStr.Substring(splittedStr.IndexOf("\"") + 1);
                splittedStr = splittedStr.Substring(0, splittedStr.IndexOf("\""));
            }
            //Console.WriteLine(splittedStr + "\n\n");
            //splittedStr = splittedStr.Split(':')[1];

            for (int i = 0; i < splittedStr.Length; i++)
            {
                if (splittedStr[i] == '"')
                {
                    isString = true;
                    break;
                }
                else if (splittedStr[i] == ' ')
                {
                    continue;
                }
                else
                {
                    break;
                }
            }

            if (isString)
            {
                splittedStr = splittedStr.Split('"')[1];
            }
            else
            {
                splittedStr = splittedStr.Split(',')[0];
            }

            int startIndex = 0;
            for (int i = 0; i < splittedStr.Length; i++)
            {
                if (splittedStr[i] == ' ')
                    startIndex++;
                else break;
            }

            return splittedStr.Substring(startIndex);
        }

        public static string ExtractIndexedField(string sourceString, string indexdFieldName)
        {
            int index = sourceString.IndexOf("\"" + indexdFieldName + "\"");
            if (index == -1)
            {
                return "ERROR_NO_INDEXED_FIELD";
            }
            string splittedStr = sourceString.Substring(index);
            int splitIndex = 0;
            int openBraces = 0;
            bool closedBraceFound = false;
            for (int i = 0; i < splittedStr.Length; i++)
            {
                if (splittedStr[i] == '[')
                {
                    openBraces++;
                }
                else if (splittedStr[i] == ']')
                {
                    closedBraceFound = true;
                    openBraces--;
                }

                if (openBraces < 1 && closedBraceFound)
                {
                    splitIndex = i;
                    break;
                }
            }

            splittedStr = splittedStr.Substring(0, splitIndex);

            return splittedStr + "]";
        }

        public static string ExtractIndexedSubField(string sourceString, int index)
        {
            int firstBrace = 0;
            int splitIndex = 0;
            int openBraces = 0;
            bool closedBraceFound = false;
            for (int i = 0; i < sourceString.Length; i++)
            {
                if (sourceString[i] == '[')
                {
                    openBraces++;
                    if (firstBrace == 0)
                        firstBrace = i + 1;
                }
                if (sourceString[i] == ']')
                {
                    closedBraceFound = true;
                    openBraces--;
                }

                if (openBraces == 0 && closedBraceFound)
                {
                    splitIndex = i;
                    break;
                }
            }

            sourceString = sourceString.Substring(firstBrace, splitIndex - firstBrace);
            //return sourceString;
            //Console.WriteLine("\n\n" + sourceString + "\n\n\n");

            string[] subStrings = sourceString.Split('}');
            List<string> strings = new List<string>();
            for (int i = 0; i < subStrings.Length; i++)
            {
                string[] strs = subStrings[i].Split('{');
                if (strs.Length > 1)
                    strings.Add(subStrings[i].Split('{')[1]);
            }
            return "{" + strings[index] + "}";
        }

        public static int GetIndexedSubFieldCount(string sourceString)
        {
            int firstBrace = 0;
            int splitIndex = 0;
            int openBraces = 0;
            bool closedBraceFound = false;
            for (int i = 0; i < sourceString.Length; i++)
            {
                if (sourceString[i] == '[')
                {
                    openBraces++;
                    if (firstBrace == 0)
                        firstBrace = i + 1;
                }
                if (sourceString[i] == ']')
                {
                    closedBraceFound = true;
                    openBraces--;
                }

                if (openBraces == 0 && closedBraceFound)
                {
                    splitIndex = i;
                    break;
                }
            }
            sourceString = sourceString.Substring(firstBrace, splitIndex - firstBrace);

            string[] subStrings = sourceString.Split('}');
            int count = 0;
            //List<string> strings = new List<string>();
            for (int i = 0; i < subStrings.Length; i++)
            {
                string[] strs = subStrings[i].Split('{');
                if (strs.Length > 1)
                    count++;
                    //strings.Add(subStrings[i].Split('{')[1]);
            }
            return count;
        }

        public static string ConstructString_FromUnicode(string unicodeString)
        {
            string result = "ERROR";

            unicodeString = unicodeString.Replace("\"", "");
            unicodeString += '\\';

            List<char> allChars = new List<char>();
            string charString = "";
            bool isReadingChar = false;
            for (int i = 0; i < unicodeString.Length; i++)
            {
                char ch = unicodeString[i];
                //if (i == unicodeString.Length - 1)
                //{
                    if ((ch == 'u' || ch == 'U' || ch == '\\' || (ch >= 48 && ch <= 57) || (ch >= 65 && ch <= 70) || (ch >= 97 && ch <= 102)) == false)
                    {
                        isReadingChar = false;
                    charString = charString.Replace("\\u", "0x");
                    try
                    {
                        char newChar = (char)Convert.ToInt32(charString, 16);
                        allChars.Add(newChar);
                    }
                    catch (Exception)
                    {
                        //Console.WriteLine("Строка имела неверный формат: " + charString + ". " + e.Message);
                    }
                    allChars.Add(ch);
                    charString = "";
                        continue;
                        //if (isReadingChar)
                        //{
                        //    //isReadingChar = false;
                        //    //charString = charString.Replace("\\u", "0x");
                        //    //char newChar = (char)Convert.ToInt32(charString, 16);
                        //    //allChars.Add(newChar);
                        //    //allChars.Add(',');
                        //    //allChars.Add(' ');
                        //    //charString = "";
                        //}
                    }
                    else
                {
                    if (isReadingChar)
                    {
                        if (ch == 'u' || ch == 'U' || (ch >= 48 && ch <= 57) || (ch >= 65 && ch <= 70) || (ch >= 97 && ch <= 102))
                        {
                            charString += ch;
                        }
                        else
                        {
                            charString = charString.Replace("\\u", "0x");
                            char newChar = (char)Convert.ToInt32(charString, 16);
                            if (ch != '\\')
                            {
                                isReadingChar = false;
                                charString = "";
                            }
                            else
                            {
                                charString = "";
                                charString += '\\';
                            }
                            allChars.Add(newChar);
                        }
                    }
                    else
                    {
                        if (ch == '\\')
                        {
                            isReadingChar = true;
                            charString += ch;
                        }
                    }
                }
                    //charString += ch;
                    //charString = charString.Replace("\\u", "0x");
                    //char newChar = (char)Convert.ToInt32(charString, 16);
                    //allChars.Add(newChar);
                    //break;
                //}

                //if (ch == '\\')
                //{
                //    if (isReadingChar)
                //    {
                //        charString = charString.Replace("\\u", "0x");
                //        char newChar = (char)Convert.ToInt32(charString, 16);
                //        allChars.Add(newChar);
                //        charString = "";
                //    }
                //    else
                //    {
                //        isReadingChar = true;
                //    }
                //    charString += '\\';
                //}
                //else if (ch == ' ')
                //{
                //    if (isReadingChar)
                //    {
                //        isReadingChar = false;
                //        charString = charString.Replace("\\u", "0x");
                //        char newChar = (char)Convert.ToInt32(charString, 16);
                //        allChars.Add(newChar);
                //        allChars.Add(' ');
                //        charString = "";
                //    }
                //}
                //else if (ch == ',')
                //{
                //    if (isReadingChar)
                //    {
                //        isReadingChar = false;
                //        charString = charString.Replace("\\u", "0x");
                //        char newChar = (char)Convert.ToInt32(charString, 16);
                //        allChars.Add(newChar);
                //        allChars.Add(',');
                //        allChars.Add(' ');
                //        charString = "";
                //    }
                //}
                //else if (ch == '.')
                //{
                //    if (isReadingChar)
                //    {
                //        isReadingChar = false;
                //        charString = charString.Replace("\\u", "0x");
                //        char newChar = (char)Convert.ToInt32(charString, 16);
                //        allChars.Add(newChar);
                //        allChars.Add('.');
                //        allChars.Add(' ');
                //        charString = "";
                //    }
                //}
                //else
                //{
                //    charString += ch;
                //}
            }
            result = new string(allChars.ToArray());

            return result;
        }
    }
}
