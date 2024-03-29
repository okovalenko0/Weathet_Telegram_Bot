﻿using System;
using System.Collections.Generic;

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
        public const string TABS_X_I = "tabs/[x]/i";

        public const string DAYS_X_SR = "days/[x]/sr";
        public const string DAYS_X_SS = "days/[x]/ss";
        public const string DAYS_X_TF = "days/[x]/tf";
        public const string DAYS_X_NF = "days/[x]/nf";

        public const string DAYS_X_SW_X_TXT = "days/[x]/sw/[0]/txt";
        public const string DAYS_X_SW_X_DESCR = "days/[x]/sw/[0]/descr";

        public const string DAYS_X_HOURS_X_I = "days/[x]/hours/[x]/i";
        public const string DAYS_X_HOURS_X_H = "days/[x]/hours/[x]/h";
        public const string DAYS_X_HOURS_X_T = "days/[x]/hours/[x]/t";
        public const string DAYS_X_HOURS_X_TF = "days/[x]/hours/[x]/tf";
        public const string DAYS_X_HOURS_X_P = "days/[x]/hours/[x]/p";
        public const string DAYS_X_HOURS_X_W = "days/[x]/hours/[x]/w";
        public const string DAYS_X_HOURS_X_WD = "days/[x]/hours/[x]/wd";
        public const string DAYS_X_HOURS_X_WS = "days/[x]/hours/[x]/ws";
        
        public const string TN = "tn";

        public const string ERROR_FIELD_NOT_EXIST = "ERROR_FIELD_NOT_EXIST";

        public const string ERROR_NO_SUB_FIELD = "ERROR_NO_SUB_FIELD";

        public static string GetStringAtPath(string sourceString, string jsonPath)
        {
            string[] tokens = jsonPath.Split('/');

            bool isBegin = false;
            bool isIndexedField = false;
            string bufferToken = "";

            for (int i = 0; i < tokens.Length; i++)
            {
                if (isBegin)
                {
                    if (isIndexedField)
                    {
                        isIndexedField = false;

                        string indexStr = tokens[i].Split('[')[1];
                        indexStr = indexStr.Split(']')[0];
                        sourceString = ExtractIndexedField(sourceString, bufferToken);
                        sourceString = ExtractIndexedSubField(sourceString, Convert.ToInt32(indexStr));
                        continue;
                    }
                    else
                    {
                        if (IsIndexedField(sourceString, tokens[i]))
                        {
                            bufferToken = tokens[i];
                            isIndexedField = true;
                        }
                        else
                        {
                            sourceString = sourceString.Substring(sourceString.IndexOf(tokens[i]) - tokens[i].Length);
                            sourceString = ExtractSubField(sourceString, tokens[i]);
                        }
                    }
                }
                else
                {
                    if (IsIndexedField(sourceString, tokens[i]))
                    {
                        bufferToken = tokens[i];
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
            return sourceString;
        }

        public static string TryGetStringAtPath(string sourceString, string jsonPath)
        {
            try
            {
                string[] tokens = jsonPath.Split('/');

                bool isBegin = false;
                bool isIndexedField = false;
                string bufferToken = "";

                for (int i = 0; i < tokens.Length; i++)
                {
                    if (isBegin)
                    {
                        if (isIndexedField)
                        {
                            isIndexedField = false;

                            string indexStr = tokens[i].Split('[')[1];
                            indexStr = indexStr.Split(']')[0];
                            sourceString = ExtractIndexedField(sourceString, bufferToken);
                            sourceString = ExtractIndexedSubField(sourceString, Convert.ToInt32(indexStr));
                            continue;
                        }
                        else
                        {
                            if (IsIndexedField(sourceString, tokens[i]))
                            {
                                bufferToken = tokens[i];
                                isIndexedField = true;
                            }
                            else
                            {
                                sourceString = sourceString.Substring(sourceString.IndexOf(tokens[i]) - tokens[i].Length);
                                sourceString = ExtractSubField(sourceString, tokens[i]);
                            }
                        }
                    }
                    else
                    {
                        if (IsIndexedField(sourceString, tokens[i]))
                        {
                            bufferToken = tokens[i];
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
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return ERROR_FIELD_NOT_EXIST;
            }
            return sourceString;
        }

        public static string[] GetAllItems(string sourceString)
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

            List<string> allFields = new List<string>();

            openBraces = 0;
            bool isReadingItem = false;
            string buffer = "";
            for (int i = 0; i < sourceString.Length; i++)
            {
                char ch = sourceString[i];
                if (isReadingItem)
                {
                    if (ch == '{')
                    {
                        openBraces++;
                    }
                    else if (ch == '}')
                    {
                        openBraces--;
                    }
                    if (openBraces <= 0)
                    {
                        allFields.Add(buffer);
                        buffer = "";
                        isReadingItem = false;
                        continue;
                    }

                    buffer += ch;
                }
                else
                {
                    if (ch == '{')
                    {
                        isReadingItem = true;
                        openBraces++;
                    }
                }
            }

            return allFields.ToArray();
        }


        public static bool IsIndexedField(string sourceString, string fieldName)
        {
            bool isSquareBracketFound = false;
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
            bool isNumber = false;
            int index = sourceString.IndexOf("\"" + subFieldName + "\"");
            if (index == -1)
            {
                return ERROR_NO_SUB_FIELD;
            }
            string splittedStr = sourceString.Substring(index + subFieldName.Length + 3);

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
                splittedStr = splittedStr.Trim(' ');
                return splittedStr;
            }
            for (int i = 0; i < splittedStr.Length; i++)
            {
                char ch = splittedStr[i];
                if (ch == '"')
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
                bool firstFound = false;
                char lastChar = ' ';
                int splitStartIndex = 0;
                int splitEndIndex = 0;
                for (int i = 0; i < splittedStr.Length; i++)
                {
                    char ch = splittedStr[i];
                    if (ch == '"' && lastChar != '\\')
                    {
                        if (firstFound)
                        {
                            splitEndIndex = i;
                            break;
                        }
                        else
                        {
                            firstFound = true;
                            splitStartIndex = i;
                        }
                    }
                    lastChar = ch;
                }
                splittedStr = splittedStr.Substring(splitStartIndex, splitEndIndex);
            }
            else
            {
                splittedStr = splittedStr.Split(',')[0];
            }
            splittedStr = splittedStr.TrimStart('\"');
            return splittedStr + " ";
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

            List<string> allFields = new List<string>();

            openBraces = 0;
            bool isReadingItem = false;
            string buffer = "";
            for (int i = 0; i < sourceString.Length; i++)
            {
                char ch = sourceString[i];
                if (isReadingItem)
                {
                    if(ch == '{')
                    {
                        openBraces++;
                    }
                    else if (ch == '}')
                    {
                        openBraces--;
                    }
                    if (openBraces <= 0)
                    {
                        allFields.Add(buffer);
                        buffer = "";
                        isReadingItem = false;
                        continue;
                    }

                    buffer += ch;
                }
                else
                {
                    if (ch == '{')
                    {
                        isReadingItem = true;
                        openBraces++;
                    }
                }
            }

            return allFields[index];
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
            
            int fieldsCount = 0;

            openBraces = 0;
            bool isReadingItem = false;
            string buffer = "";
            for (int i = 0; i < sourceString.Length; i++)
            {
                char ch = sourceString[i];
                if (isReadingItem)
                {
                    if (ch == '{')
                    {
                        openBraces++;
                    }
                    else if (ch == '}')
                    {
                        openBraces--;
                    }
                    if (openBraces <= 0)
                    {
                        fieldsCount++;
                        buffer = "";
                        isReadingItem = false;
                        continue;
                    }

                    buffer += ch;
                }
                else
                {
                    if (ch == '{')
                    {
                        isReadingItem = true;
                        openBraces++;
                    }
                }
            }

            return fieldsCount;
        }

        public static string ConstructString_FromUnicode(string unicodeString)
        {
            string result = "ERROR";
            List<char> allChars = new List<char>();
            string charString = "";
            bool isReadingChar = false;
            for (int i = 0; i < unicodeString.Length; i++)
            {
                char ch = unicodeString[i];
                if ((ch == 'u' || ch == 'U' || ch == '\\' || (ch >= 48 && ch <= 57) || (ch >= 65 && ch <= 70) || (ch >= 97 && ch <= 102)) == false)
                {
                    isReadingChar = false;
                charString = charString.Replace("\\u", "0x");
                try
                {
                    char newChar = (char)Convert.ToInt32(charString, 16);
                    allChars.Add(newChar);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Строка имела неверный формат: " + charString + ". " + e.Message);
                }
                allChars.Add(ch);
                charString = "";
                    continue;
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
                        else allChars.Add(ch);
                    }
                }
               
            }
            result = new string(allChars.ToArray());
            return result;
        }
    }
}