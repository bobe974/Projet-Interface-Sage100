using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Test.Classes
{
    class FtpSync
    {





        /**
           * Etabli un connexion FTP  puis télécharge plusieurs fichiers suivant la liste donnée en paramètre (
           */
        public void DownloadMultipleFilesFromFtp(String user, String pwd, String url, List<string> listFilesPath, string localFolderPath)
        {
            //établir une connexion FTP permanente
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);
            request.Credentials = new NetworkCredential(user, pwd);
            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            Stream responseStream = response.GetResponseStream();

            //parcourir la liste des fichiers à télécharger
            foreach (string remoteFilePath in listFilesPath)
            {
                //construire l'URL complète du fichier distant
                string fileUrl = Path.Combine(url, remoteFilePath);

                //créer le chemin complet du fichier local
                string destPath = Path.Combine(localFolderPath, Path.GetFileName(remoteFilePath));

                //télécharger le fichier distant
                DownloadFileFromFtp(request, responseStream, fileUrl, destPath);
            }

            //fermer la connexion FTP
            response.Close();
        }

        /**
         * Se connecte à un serveur par protocole FTP et télécharge le fichiers lié à l'url
         */
        public void DownloadFileFromFtp(FtpWebRequest request, Stream responseStream, string fileUrl, string destPath)
        {
            //créer une nouvelle requête pour télécharger le fichier spécifié
            FtpWebRequest downloadRequest = (FtpWebRequest)WebRequest.Create(fileUrl);
            downloadRequest.Method = WebRequestMethods.Ftp.DownloadFile;
            downloadRequest.Credentials = request.Credentials; // réattribue les paramatres de connexion

            //télécharger le fichier et l'écrire dans le fichier local
            using (FtpWebResponse response = (FtpWebResponse)downloadRequest.GetResponse())
            {
                using (Stream remoteStream = response.GetResponseStream())
                {
                    using (FileStream localStream = new FileStream(destPath, FileMode.Create))
                    {
                        remoteStream.CopyTo(localStream);
                    }
                }
            }
            Console.WriteLine($"Téléchargement de {fileUrl} terminé.");
        }
    }

}


