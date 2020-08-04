using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace JSONtoHTMLConverter
{
    class HTMLFile
    {
        private void AddAttributes(JToken attributes, ref string result)
        {
            // Metoda za dodajanje attributov k html znački
            // Sprehod čez vse atribute in dodajanje k rezultatu
            foreach(JToken attribute in attributes.Children<JProperty>())
            {
                string attributeName = ((JProperty)attribute).Name;
                result = result + " ";
                result = result + attributeName;
                result = result + "=\"";

                // Posebna izjema za atribut style, kjer se je potrebno dodatno sprehoditi
                // čez vse elemente
                if(attributeName == "style")
                {
                    foreach (JToken child in attribute.Children().Children())
                    {
                        result = result + ((JProperty)child).Name;
                        result = result + ":";
                        result = result + ((JProperty)child).Value;
                        result = result + ";";
                    }
                }
                else
                {
                    result = result + ((JProperty)attribute).Value;
                }

                result = result + "\"";
            }
        }

        private void AppendTag(JToken tag, ref string result)
        {
            string tagName = tag.Path;
            tagName = tagName.Substring(tagName.LastIndexOf('.') + 1);

            // Malo nerodno, da moram to preverjati, ampak ta if je potreben
            // da se ne ustvari značka <attributes>
            if(tagName != "attributes")
            {
                result = result + "\n<" + tagName;
            }

            // Tukaj se preveri ali ima značka kake atribute, ki se potem
            // dodajo z metodo AddAttributes
            if(tag.Type == JTokenType.Object)
            {
                JObject tagObject = tag.ToObject<JObject>();

                if(tagObject.ContainsKey("attributes") == true)
                {
                    AddAttributes(tagObject["attributes"], ref result);
                }
            }

            // Podobno kot zgoraj, nočemo v rezultatu imeti značke
            if (tagName != "attributes")
            {
                result = result + ">";

                if (tag.Type == JTokenType.String)
                {
                    result = result + tag.Parent.Last.ToString();
                }
                else
                {
                    foreach (JToken child in tag.Children())
                    {
                        AppendTag(child.Last, ref result);
                    }
                }

                result = result + "\n</" + tagName + ">";
            }
        }

        private void AppendHead(JToken head, ref string result)
        {
            result = result + "\n<head>";
            foreach(JToken property in head.Children())
            {
                string tagName = property.Path;
                tagName = tagName.Substring(tagName.LastIndexOf('.') + 1);

                if (tagName == "link")
                {
                    JArray links = property.Last.ToObject<JArray>();
                    foreach(JToken link in links)
                    {
                        result = result + "\n<link";
                        AddAttributes(link, ref result);
                        result = result + ">";
                    }
                }
                else if (tagName == "meta")
                {
                    foreach (JProperty metaProperty in property.Last.Children())
                    {
                        result = result + "\n<meta ";
                        if (metaProperty.Name == "charset")
                        {
                            result = result + "charset=\"" + metaProperty.Value + "\"";
                        }
                        else
                        {
                            result = result + "name=\"" + metaProperty.Name + "\" content=" + "\"" + metaProperty.Value + "\"";
                        }
                        result = result + ">";
                    }
                }
                else
                {
                    AppendTag(property.Last, ref result);
                }
            }
            result = result + "\n</head>";
        }

        public void Convert(string json, string outputFile)
        {
            JObject root = JObject.Parse(json);

            string doctype = root.ContainsKey("doctype") ? (string)root["doctype"] : "html";
            string language = root.ContainsKey("language") ? (string)root["language"] : "en";

            string result = "<!DOCTYPE " + doctype + ">\n<html lang=\"" + language + "\">";

            // generiranje glave 
            if (root.ContainsKey("head"))
            {
                JToken head = root["head"];

                AppendHead(head, ref result);
            }
            //

            // generiranje telesa
            if (root.ContainsKey("body"))
            {
                JToken body = root["body"];

                AppendTag(body, ref result);
            }
            //

            result = result + "\n</html>";

            File.WriteAllText(outputFile, result);
        }
    }
}
