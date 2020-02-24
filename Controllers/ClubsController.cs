using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Xml;

namespace FootballClubsServer.Controllers
{
    public class ClubsController : Controller
    {
        private readonly ILogger<ClubsController> _logger;
        private readonly string imagesDirectory;
        private IWebHostEnvironment env;

        public ClubsController(IWebHostEnvironment environment, ILogger<ClubsController> logger)
        {
            env = environment;
            imagesDirectory = env.WebRootPath + "\\Images\\";
            _logger = logger;
        }

        [HttpPost]
        [Route("Clubs/Add/{id:int}/{name}")]
        public HttpResponseMessage Add(int id, string name)
        {
            HttpResponseMessage httpResponse = new HttpResponseMessage();

            string logoPath = imagesDirectory + name + "_logo.png";

            FileStream logoStream = null;
            try
            {
                logoStream = new FileStream(logoPath, FileMode.Create, FileAccess.Write);
                Stream str = Request.Form.Files[0].OpenReadStream();
                str.CopyTo(logoStream);
                str.Close();
            }
            catch (Exception e)
            {
                httpResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                httpResponse.ReasonPhrase = e.Message;
                return httpResponse;
            }
            finally
            {
                logoStream.Close();
            }

            FootballClub newClub = new FootballClub(id, name, logoPath);
            FootballClub.clubs.Add(newClub);
            AppendToXml(newClub);

            httpResponse.StatusCode = System.Net.HttpStatusCode.OK;
            httpResponse.ReasonPhrase = "Данные сохранены";

            return httpResponse;
        }

        internal void AppendToXml(FootballClub newClub)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load("clubs.xml");
            XmlElement xRoot = xDoc.DocumentElement;

            XmlElement clubElem = xDoc.CreateElement("club");
            XmlAttribute idAttr = xDoc.CreateAttribute("id");
            XmlElement nameElem = xDoc.CreateElement("name");
            XmlElement imagePathElem = xDoc.CreateElement("imagePath");

            XmlText idText = xDoc.CreateTextNode(newClub.Id.ToString());
            XmlText nameText = xDoc.CreateTextNode(newClub.Name);
            XmlText imagePathText = xDoc.CreateTextNode(newClub.ImagePath);

            idAttr.AppendChild(idText);
            nameElem.AppendChild(nameText);
            imagePathElem.AppendChild(imagePathText);
            clubElem.Attributes.Append(idAttr);
            clubElem.AppendChild(nameElem);
            clubElem.AppendChild(imagePathElem);

            xRoot.AppendChild(clubElem);
            xDoc.Save("clubs.xml");
        }

        [HttpPut]
        [Route("Clubs/Edit/{id:int}")]
        public HttpResponseMessage Edit(int id)
        {
            HttpResponseMessage httpResponse = new HttpResponseMessage();

            StreamReader nameReader = null;
            string name = null;
            string logoPath = null;
            FileStream logoStream = null;
            Stream str = null;
            try
            {
                nameReader = new StreamReader(Request.Form.Files[0].OpenReadStream());
                name = nameReader.ReadToEnd();

                logoPath = imagesDirectory + name + "_logo.png";

                logoStream = new FileStream(logoPath, FileMode.Create, FileAccess.Write);
                str = Request.Form.Files[1].OpenReadStream();
                str.CopyTo(logoStream);
            }
            catch (Exception e)
            {
                httpResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
                httpResponse.ReasonPhrase = e.Message;
            }
            finally
            {
                nameReader.Close();
                str.Close();
                logoStream.Close();
            }


            FootballClub clubToEdit = FootballClub.clubs.First(c => c.Id == id);
            clubToEdit.Name = name;
            clubToEdit.ImagePath = logoPath;

            EditInXml(clubToEdit);

            httpResponse.StatusCode = System.Net.HttpStatusCode.OK;

            return httpResponse;
        }

        internal void EditInXml(FootballClub club)
        {
            XmlDocument xDoc = new XmlDocument();
            xDoc.Load("clubs.xml");
            XmlElement xRoot = xDoc.DocumentElement;

            foreach(XmlNode node in xRoot)
            {
                if(node.Attributes.Count > 0)
                {
                    if(node.Attributes.GetNamedItem("id").Value == club.Id.ToString())
                    {
                        node.RemoveAll();

                        XmlAttribute idAttr = xDoc.CreateAttribute("id");
                        XmlElement nameElem = xDoc.CreateElement("name");
                        XmlElement imagePathElem = xDoc.CreateElement("imagePath");

                        XmlText idText = xDoc.CreateTextNode(club.Id.ToString());
                        XmlText nameText = xDoc.CreateTextNode(club.Name);
                        XmlText imagePathText = xDoc.CreateTextNode(club.ImagePath);

                        idAttr.AppendChild(idText);
                        nameElem.AppendChild(nameText);
                        imagePathElem.AppendChild(imagePathText);
                        node.Attributes.Append(idAttr);
                        node.AppendChild(nameElem);
                        node.AppendChild(imagePathElem);

                        break;
                    }
                }
            }
            xDoc.Save("clubs.xml");
        }

        [HttpDelete]
        [Route("Clubs/Remove/{id:int}")]
        public HttpResponseMessage Remove(int id)
        {
            HttpResponseMessage httpResponse = new HttpResponseMessage(System.Net.HttpStatusCode.OK);

            FootballClub clubToRemove = FootballClub.clubs.First(c => c.Id == id);

            XmlDocument xDoc = new XmlDocument();
            xDoc.Load("clubs.xml");
            XmlElement xRoot = xDoc.DocumentElement;

            foreach (XmlNode node in xRoot)
            {
                if (node.Attributes.Count > 0)
                {
                    XmlNode attr = node.Attributes.GetNamedItem("id");
                    if (attr.Value == id.ToString())
                    {
                        xRoot.RemoveChild(node);
                        break;
                    }
                }
            }
            xDoc.Save("clubs.xml");

            string logoPath = imagesDirectory + clubToRemove.Name + "_logo.png";
            if (System.IO.File.Exists(logoPath))
                System.IO.File.Delete(logoPath);

            FootballClub.clubs.Remove(clubToRemove);

            return httpResponse;
        }

        [HttpGet]
        [Route("Clubs/Load")]
        public List<FootballClub> Load()
        {
            return FootballClub.clubs;
        }

        [HttpGet]
        [Route ("Clubs/GetLogo/{id:int}")]
        public FileResult GetLogo(int id)
        {
            byte[] mas = System.IO.File.ReadAllBytes(FootballClub.clubs.First(c => c.Id == id).ImagePath);
            return File(mas, "image/png");
        }
    }
}
