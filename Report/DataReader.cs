using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Reflection;
using System.Globalization;

namespace ReportLibrary
{

        public class DataSourceHelper
        {
            // Methods
            private DataSourceHelper()
            {
            }
            public static IEnumerable GetResolvedDataSource(object dataSource, string dataMember)
            {
                if (dataSource != null)
                {
                    IListSource source1 = dataSource as IListSource;
                    if (source1 != null)
                    {
                        IList list1 = source1.GetList();
                        if (!source1.ContainsListCollection)
                        {
                            return list1;
                        }
                        if ((list1 != null) && (list1 is ITypedList))
                        {
                            ITypedList list2 = (ITypedList)list1;
                            PropertyDescriptorCollection collection1 = list2.GetItemProperties(new PropertyDescriptor[0]);
                            if ((collection1 == null) || (collection1.Count == 0))
                            {
                                return null;
                            }
                            PropertyDescriptor descriptor1 = null;
                            if ((dataMember == null) || (dataMember.Length == 0))
                            {
                                descriptor1 = collection1[0];
                            }
                            else
                            {
                                descriptor1 = collection1.Find(dataMember, true);
                            }
                            if (descriptor1 != null)
                            {
                                object obj1 = list1[0];
                                object obj2 = descriptor1.GetValue(obj1);
                                if ((obj2 != null) && (obj2 is IEnumerable))
                                {
                                    return (IEnumerable)obj2;
                                }
                            }
                            return null;
                        }
                    }
                    if (dataSource is IEnumerable)
                    {
                        return (IEnumerable)dataSource;
                    }
                }
                return null;
            }

            private static object Eval(object container, string[] expressionParts)
            {
                object obj1 = container;
                for (int num1 = 0; (num1 < expressionParts.Length) && (obj1 != null); num1++)
                {
                    string text1 = expressionParts[num1];
                    if (text1.IndexOfAny(DataSourceHelper.indexExprStartChars) < 0)
                    {
                        obj1 = DataSourceHelper.GetPropertyValue(obj1, text1);
                    }
                    else
                    {
                        obj1 = DataSourceHelper.GetIndexedPropertyValue(obj1, text1);
                    }
                }
                return obj1;
            }

            public static object Eval(object container, string expression)
            {
                if (expression == null)
                {
                    return null;
                }
                if (container == null)
                {
                    return null;
                }
                if (container is System.Data.DataRow)
                {
                    System.Data.DataRow dtr = (container as System.Data.DataRow);
                    return dtr[expression];
                }

                string[] textArray1 = expression.Trim().Split(DataSourceHelper.expressionPartSeparator);
                return DataSourceHelper.Eval(container, textArray1);
            }

            public static string Eval(object container, string expression, string format)
            {
                object obj1 = DataSourceHelper.Eval(container, expression);
                if ((obj1 == null) || (obj1 == DBNull.Value))
                {
                    return string.Empty;
                }
                if ((format != null) && (format.Length != 0))
                {
                    return string.Format(format, obj1);
                }
                return obj1.ToString();
            }

            public static object GetIndexedPropertyValue(object container, string expr)
            {
                if (container == null)
                {
                    return null;
                }
                if ((expr == null) || (expr.Length == 0))
                {
                    return null;
                }
                object obj1 = null;
                bool flag1 = false;
                int num1 = expr.IndexOfAny(DataSourceHelper.indexExprStartChars);
                int num2 = expr.IndexOfAny(DataSourceHelper.indexExprEndChars, num1 + 1);
                if (((num1 < 0) || (num2 < 0)) || (num2 == (num1 + 1)))
                {
                    return null;
                }
                string text1 = null;
                object obj2 = null;
                string text2 = expr.Substring(num1 + 1, (num2 - num1) - 1).Trim();
                if (num1 != 0)
                {
                    text1 = expr.Substring(0, num1);
                }
                if (text2.Length != 0)
                {
                    if (((text2[0] == '"') && (text2[text2.Length - 1] == '"')) || ((text2[0] == '\'') && (text2[text2.Length - 1] == '\'')))
                    {
                        obj2 = text2.Substring(1, text2.Length - 2);
                    }
                    else if (char.IsDigit(text2[0]))
                    {
                        try
                        {
                            obj2 = int.Parse(text2, CultureInfo.InvariantCulture);
                            flag1 = true;
                        }
                        catch (Exception)
                        {
                            obj2 = text2;
                        }
                    }
                    else
                    {
                        obj2 = text2;
                    }
                }
                if (obj2 == null)
                {
                    return null;
                }
                object obj3 = null;
                if ((text1 != null) && (text1.Length != 0))
                {
                    obj3 = DataSourceHelper.GetPropertyValue(container, text1);
                }
                else
                {
                    obj3 = container;
                }
                if (obj3 == null)
                {
                    return obj1;
                }
                if ((obj3 is Array) && flag1)
                {
                    return ((object[])obj3)[(int)obj2];
                }
                if ((obj3 is IList) && flag1)
                {
                    return ((IList)obj3)[(int)obj2];
                }
                Type[] typeArray1 = new Type[1] { obj2.GetType() };
                PropertyInfo info1 = obj3.GetType().GetProperty("Item", BindingFlags.Public | BindingFlags.Instance, null, null, typeArray1, null);
                if (info1 != null)
                {
                    object[] objArray1 = new object[1] { obj2 };
                    return info1.GetValue(obj3, objArray1);
                }
                return null;
            }

            public static object GetFieldValue(object container, string fldName)
            {
                FieldInfo fld = container.GetType().GetField(fldName);
                if (fld != null)
                {
                    return fld.GetValue(container);
                }
                return null;
            }

            public string GetFieldValue(object container, string fldName, string format)
            {
                FieldInfo fld = container.GetType().GetField(fldName);
                if (fld != null)
                {
                    object obj1 = fld.GetValue(container);
                    if ((obj1 == null) || (obj1 == DBNull.Value))
                    {
                        return string.Empty;
                    }
                    if ((format != null) && (format.Length != 0))
                    {
                        return string.Format(format, obj1);
                    }
                }
                return null;
            }

            public static string GetIndexedPropertyValue(object container, string propName, string format)
            {
                object obj1 = DataSourceHelper.GetIndexedPropertyValue(container, propName);
                if ((obj1 == null) || (obj1 == DBNull.Value))
                {
                    return string.Empty;
                }
                if ((format != null) && (format.Length != 0))
                {
                    return string.Format(format, obj1);
                }
                return obj1.ToString();
            }

            public static object GetPropertyValue(object container, string propName)
            {
                if (container == null)
                {
                    return null;
                }
                if ((propName == null) || (propName.Length == 0))
                {
                    return null;
                }
                PropertyDescriptor descriptor1 = TypeDescriptor.GetProperties(container).Find(propName, true);
                if (descriptor1 == null)
                {
                    return GetFieldValue(container, propName);
                    //throw new Exception("Binding Exception");
                }
                return descriptor1.GetValue(container);
            }

            public static string GetPropertyValue(object container, string propName, string format)
            {
                object obj1 = DataSourceHelper.GetPropertyValue(container, propName);
                if ((obj1 == null) || (obj1 == DBNull.Value))
                {
                    return string.Empty;
                }
                if ((format != null) && (format.Length != 0))
                {
                    return string.Format(format, obj1);
                }
                return obj1.ToString();
            }

            static DataSourceHelper()
            {
                char[] chArray1 = new char[1] { '.' };
                DataSourceHelper.expressionPartSeparator = chArray1;
                chArray1 = new char[2] { '[', '(' };
                DataSourceHelper.indexExprStartChars = chArray1;
                chArray1 = new char[2] { ']', ')' };
                DataSourceHelper.indexExprEndChars = chArray1;
            }
            private static readonly char[] expressionPartSeparator;
            private static readonly char[] indexExprEndChars;
            private static readonly char[] indexExprStartChars;
        }

}
