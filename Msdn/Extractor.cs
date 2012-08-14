using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Forms;
namespace Msdn
{
    class Extractor : WebClient
    {
        string htmlCode, Carpeta, titulo;

        private string ObtenerTitulo(string datos)
        {
            var d = new XmlDocument();
            d.LoadXml(datos);
            return d["head"]["title"].InnerText;
        }
        public Extractor(Uri Url)
        {
            this.Encoding = Encoding.UTF8;
            var enlaces = new XDocument(new XDeclaration("1.0", "utf-8", null));
            var e = new XElement("paginas");
            e.Add(CrearNodoPag("", Url.AbsolutePath, e.GetHashCode().ToString()));
            InsertarPaginas(e, OtenerDocumento(Url, e.GetHashCode().ToString()), e.GetHashCode().ToString());
            ((XElement)e.FirstNode).Attribute("id").Value = e.GetHashCode().ToString();
            enlaces.Add(e);
            var sb = new StringBuilder();
            var d = ((XElement)e.FirstNode).Attribute("id");
            sb.Append(ObetenerDiv(this.DownloadString(Carpeta + ((XElement)e.FirstNode).Attribute("id").Value + ".html"), "html", "head"));
            titulo = ObtenerTitulo(sb.ToString()).Trim();
            var lista = e.CreateReader();
            while (lista.Read())
            {
                if (lista.Name == "paginas") continue;
                sb.Append(ObetenerDiv(this.DownloadString(Carpeta + lista.GetAttribute("id") + ".html"),
                    "topicContainer", "div").Replace("class=\"title\"", "class=\"title\" " + "toc=\"" + 
                    ObtenerDimencion(e, int.Parse(lista.GetAttribute("padre")))[1] + "\""));
            }
            
            var f = File.CreateText(Carpeta + "resultado.html");
            f.Write(sb.ToString());
            f.Close();
            f.Dispose();
            sb.Clear();
            var guardar = new SaveFileDialog();
            guardar.Filter = "HTML Página|*.html";
            guardar.Title = "Guardar " + titulo;
            guardar.FileName = titulo;
            guardar.ShowDialog();
            if (guardar.FileName != "")
            {
                var fr = File.OpenText(Carpeta + "resultado.html");
                f = File.CreateText(guardar.FileName);
                f.Write(fr.ReadToEnd());
                f.Close();
                f.Dispose();
                fr.Close();
                fr.Dispose();
            }
            Directory.Delete(Carpeta, true);                     
        }
        private int[] ObtenerDimencion(XNode Contenido, int hashPadre)
        {
            if (Contenido.GetHashCode() == hashPadre) return new int[] { hashPadre, 1 };
            var r = Contenido.CreateReader();
            while (r.Read())
            {
                if (r.GetAttribute("id") == hashPadre.ToString())
                {
                    var ra = ObtenerDimencion(Contenido, int.Parse(r.GetAttribute("padre")));
                    ra[1] += 1;
                    return ra;
                }
            }
            return new int[] { hashPadre, 1 };
        }
        private XmlNode OtenerDocumento(Uri Url, String Hash)
        {
            Carpeta = AppDomain.CurrentDomain.BaseDirectory + @"\Teporales\";
            Directory.CreateDirectory(Carpeta);
            this.DownloadFile(Url, Carpeta + Hash + ".html");
            // Or you can get the file content without saving it:
            htmlCode = this.DownloadString(Carpeta + Hash + ".html");
            var d = new XmlDocument();            
            d.LoadXml(ObetenerDiv(htmlCode, "toclevel2", "div"));
            return d.ChildNodes[0];
        }
        private bool InsertarPaginas<T, U>( T elemento, U datos, string Hash)
            where T : XElement
            where U : XmlNode
        {
            if (datos.ChildNodes.Count < 2) return false;
            foreach (XmlNode valor in datos)
            {
                if (valor.Attributes.Count == 0)
                {
                    
                    elemento.Add(CrearNodoPag(valor.FirstChild.Attributes["title"].Value, valor.FirstChild.Attributes["href"].Value, Hash));
                    InsertarPaginas(((XElement)elemento), OtenerDocumento(new Uri(@"http://msdn.microsoft.com" + ((XElement)elemento.LastNode).Attribute("Url").Value), elemento.LastNode.GetHashCode().ToString()), elemento.LastNode.GetHashCode().ToString());
                        
                        
                }
            }
            return true;
        }
        private XElement CrearNodoPag(string Nombre, String Url, String padre)
        {
            var d = new XElement("pagina", new XAttribute("Nombre", Nombre),
                new XAttribute("Url", Url), new XAttribute("padre", padre));
            d.Add(new XAttribute("id", d.GetHashCode().ToString()));
            return d;
        }
        private string ObetenerDiv(string datos, string Etiquta, string Atributo)
        {
            int comienzo = datos.IndexOf(Etiquta), puntero = comienzo;
            if (comienzo == -1)
            {
                return "<nada />";
            }
            int vC = datos.LastIndexOf("<" + Atributo, comienzo - (Atributo.Length + 2));
            if (vC == -1) vC = datos.IndexOf("<" + Atributo, comienzo - (Atributo.Length + 2));
            
            int contP = 1, contF = 0;
            do
            {
                int p = datos.IndexOf("<" + Atributo, puntero) + (Atributo.Length + 3);
                int f = datos.IndexOf("</" + Atributo, puntero) + (Atributo.Length + 3);
                if (p < f && p - (Atributo.Length + 3) != -1)
                {
                    contP += 1;
                    puntero = p;
                }
                else if (f - (Atributo.Length + 3) != -1)
                {
                    contF += 1;
                    puntero = f;
                }
                else break;
            } while(contP != contF);
            return datos.Substring(vC, puntero - vC);
        }
    }
}
