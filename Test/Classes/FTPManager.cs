using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace Test.Classes
{
    class FTPManager
    {
        private FtpWebRequest ftpRequest;
        private String user;
        private String pwd;
        private String url;


        public FTPManager(String user, String pwd, String url)
        {
            this.user = user;
            this.pwd = pwd;
            this.url = url;
        }

        //TODO Une instance de ftpRequest pour toutes les opérations

        public void OpenFtpConnection()
        {
            try{
                ftpRequest = (FtpWebRequest)WebRequest.Create(url);
                ftpRequest.Credentials = new NetworkCredential(user, pwd);
                ftpRequest.UseBinary = true;
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails; //liste détaillé des répertoires 
                FtpWebResponse response = (FtpWebResponse)ftpRequest.GetResponse();

                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                Console.WriteLine(reader.ReadToEnd());

                Console.WriteLine($"Directory List Complete, status {response.StatusDescription}");
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
            

        }

        /**
         * Etabli une connexion FTP puis synchonise les fichiers du répertoire local au répertoire distant
         * Télécharge les fichiers qui n'existe pas en local ou qui ont été mise à jours
         */
        public void SyncFilesFromFtp(string localFolderPath)
        {
            Console.WriteLine("Tentative de connexion au serveur par protocole FTP...");
            try
            {
                //établir une connexion FTP
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);
                request.Credentials = new NetworkCredential(user, pwd);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
               
                //vérifie si la connexion par FTP fonctionne
                FtpWebResponse response = null;
                bool isConnected = false;            
                while (!isConnected)
                {
                    try
                    {
                        //TODO pas optimal mais ca marche
                        //établir une connexion FTP
                        FtpWebRequest newrequest = (FtpWebRequest)WebRequest.Create(url);
                        request = newrequest;
                        request.Credentials = new NetworkCredential(user, pwd);
                        request.Method = WebRequestMethods.Ftp.ListDirectory;
                        response = (FtpWebResponse)request.GetResponse();
                        Console.WriteLine("connexion FTP établie");
                        isConnected = true;
                    }
                    catch (WebException e)
                    {
                        // Attendre 5 secondes avant de retenter la connexion
                        Thread.Sleep(5000);
                        Console.WriteLine("pas de connexion FTP, nouvelle tentative dans 5s");
                        Console.WriteLine("erreur -> "+ e+"\n");
                    }
                }
                //liste des fichiers distants
                List<string> remoteFiles = new List<string>();
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string line = reader.ReadLine();
                    Console.WriteLine("line:" + line);
                    while (!string.IsNullOrEmpty(line))  //pas de ficiers vide ou qui ne sont pas des .json
                    {
                        if (!(IsNotJson(line))) // si c'est un json on l'ajoute à la liste des téléchargements
                        {
                            remoteFiles.Add(line);   
                        }
                      line = reader.ReadLine(); //lecture de la ligne suivante
                    }
                }

                //liste des fichiers locaux
                List<string> localFiles = Directory.GetFiles(localFolderPath).ToList();

                //synchroniser les fichiers
                foreach (string remoteFilePath in remoteFiles)
                {
                    string fileName = Path.GetFileName(remoteFilePath);
                    string localFilePath = Path.Combine(localFolderPath, fileName); //path dossier local + nom fichier distant
                    Console.WriteLine("remote filepath:" + remoteFilePath);
                    if (localFiles.Contains(localFilePath))
                    {
                        //cas fichier existant, vérifier la taille 
                        long remoteFileSize = GetFileSize(request, remoteFilePath);
                        long localFileSize = new FileInfo(localFilePath).Length;
                        Console.WriteLine("taille du fichier local: " + localFileSize);
                        Console.WriteLine("taille du fichier distant: " + remoteFileSize);
                        if (remoteFileSize != localFileSize)
                        {
                            //télécharger la version la plus récente du fichier
                            Console.WriteLine("Fichier existant, dl de la version la plus récente de: " + fileName);
                            DownloadFileFromFtp(request, remoteFilePath, localFilePath);
                        }
                    }
                    else
                    {
                        //cas d'un ouveau fichier, le télécharger
                        Console.WriteLine("Fichier inexistant, téléchargement de: " + fileName);
                        DownloadFileFromFtp(request, remoteFilePath, localFilePath);
                    }
                }
                response.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }        
        }


        /**
         * Se connecte à un serveur par protocole FTP et télécharge le fichiers lié à l'url
         */
        public void DownloadFileFromFtp(FtpWebRequest request, string fileUrl, string destPath)
        {
            Console.WriteLine("format de l'uri download: " + fileUrl);
            String path = this.url + "/" + fileUrl;
            //créer une nouvelle requête pour télécharger le fichier spécifié
            FtpWebRequest downloadRequest = (FtpWebRequest)WebRequest.Create(path);
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


        /**
         * 
         */
        public long GetFileSize(FtpWebRequest request, string fileUrl)
        {

            String path = this.url + "/" + fileUrl;
            Console.WriteLine("file uri: " + path);
            FtpWebRequest sizeRequest = (FtpWebRequest)WebRequest.Create(path);
            sizeRequest.Credentials = new NetworkCredential(user, pwd);
            sizeRequest.Method = WebRequestMethods.Ftp.GetFileSize;
            sizeRequest.UseBinary = true;

            using (FtpWebResponse response = (FtpWebResponse)sizeRequest.GetResponse())
            {
                return response.ContentLength;
            }
        }

        /**
           * Etabli un connexion FTP  puis télécharge plusieurs fichiers suivant la liste donnée en paramètre
           */
        public void DownloadMultipleFilesFromFtp(List<string> listFilesPath, string localFolderPath)
        {
            //établir une connexion FTP permanente
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);
            request.Credentials = new NetworkCredential(user, pwd);
            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                using (Stream responseStream = response.GetResponseStream())
                {
                    //parcourir la liste des fichiers à télécharger
                    foreach (string remoteFilePath in listFilesPath)
                    {
                        //construire l'URL complète du fichier distant
                        Uri fileUri = new Uri(new Uri(url), remoteFilePath);
                        string fileUrl = fileUri.ToString();

                        //créer le chemin complet du fichier local
                        string destPath = Path.Combine(localFolderPath, Path.GetFileName(remoteFilePath));

                        //télécharger le fichier distant
                        DownloadFileFromFtp(request, fileUrl, destPath);
                    }
                }
            }
        }

        public bool IsNotJson(string filePath)
        { 
            string extension = Path.GetExtension(filePath);
            if (extension == ".json")
            {
                Console.WriteLine(filePath + " est un json");
                return false;
            }
            else
            {
                Console.WriteLine(filePath + " n'est pas un json");
                return true;
            }
        }
    }
}


