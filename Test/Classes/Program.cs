using System;
using Newtonsoft.Json;
using System.IO;
using Test.Classes;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Test

{
    class Program
    {

        //constantes pour la connexion FTP
        public const string USER = "etienne";
        public const string PWD = "root";
        public const string URL = "ftp://127.0.0.1:21";
        //emplacement où seront stockés les fichiers json téléchargés
        public const string LOCALFILEPATH = @"C:\Users\Utilisateur\Desktop\fichierLocal";
        //fichier pour stocker les noms de fichiers traités
        public const string processedFilesPath = @"C:\Users\Utilisateur\Desktop\fichierLocal\processFile.txt";
        //base de données SAGE
        public const string DBCOMPTAPATH = @"C:\Users\Utilisateur\Desktop\Projet 1\test\STOCKSERVICE.mae";
        public const string DBCOMPTAUSER = "<Administrateur>";
        public const string DBCOMPTAPWD = "AR2003";

        public const string DBCIALPATH = @"C:\Users\Utilisateur\Desktop\Projet 1\test\STOCKSERVICE.gcm";
        public const string DBCIALUSER = "<Administrateur>";
        public const string DBCIALPWD = "AR2003";

        static void Main(string[] args)
        {
            //Connexion et récupération des json par FTP           
            FTPManager ftp = new FTPManager(USER, PWD, URL);
            ftp.SyncFilesFromFtp(LOCALFILEPATH);
            Console.ReadLine();

            //Paramètres pour se connecter aux bases
            ParamDb paramBaseCompta = new ParamDb(DBCOMPTAPATH, DBCOMPTAUSER, DBCOMPTAPWD);
            ParamDb paramBaseCial = new ParamDb(DBCIALPATH, DBCIALUSER, DBCIALPWD);

            //ouverture des bases de données
            Console.WriteLine("tentative de connexion a la base SAGE...");
            SageCommandeManager sage = new SageCommandeManager(paramBaseCompta, paramBaseCial);

            if (sage.isconnected)
            {
                Console.ReadLine();
                //parcours des fichiers json dans le répertoire
                List<string> localFiles = Directory.GetFiles(LOCALFILEPATH).ToList();
                foreach (string s in localFiles)
                {
                    Console.WriteLine("*******************************************Bon de commande*******************************************");
                    Console.WriteLine(s);
                    if (!(IsFileAlreadyProcessed(s))) // vérifie si le fichier a deja été traité
                    {
                        // génère un objet JsonModel depuis le fichier json
                        Console.WriteLine("conversion de " + s + " en modele C#");
                        if (ValidateJson(s))
                        {
                            try
                            {
                                JsonModel jsonModel = DeserialiseJson(s);
                                ValidateInputData(jsonModel);
                                //créer le bon de commande 
                                if (sage.Createcmd(jsonModel))
                                {
                                    //ajoute le nom du ficher dans la liste des fichiers traités
                                    AddFileAsProcessed(s);
                                }                        
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine($"Erreur lors de la validation du modèle de données JSON d'entrée pour le fichier {s}: {e.Message}");
                            }
                        }   
                    }
                }
            }
            Console.ReadLine(); 
            sage.CloseDB();
            Console.ReadLine();

            /*********************************************************Méthodes***********************************************************/

            /**
             * Converti un fichier json en objet C#
             */
            JsonModel DeserialiseJson(string path)
            {
                try
                {
                    Console.WriteLine("Lecture du fichier Json:" + path + "\n");
                    string json = File.ReadAllText(path);
                    //conversion du json en Objet c#
                    JsonModel obj = JsonConvert.DeserializeObject<JsonModel>(json);
                    return obj;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return null;
                }
            }

            /**
             * Vérifie si le fichier processed existe, dans la cas contraire, on créer le fichier
             */
            void CheckProcessFile()
            {
                //vérifier si le fichier processfilesPath existe
                if (!File.Exists(processedFilesPath))
                {
                    Console.WriteLine("création du fichier process");
                    //créer le fichier
                    using (StreamWriter writer = new StreamWriter(processedFilesPath))
                    {
                        Console.WriteLine("fichier process crée");
                    }
                }
            }

            /**
             * Vérifie si le fichier a déjà été traité en vérifiant s'il figure dans la liste des fichiers traités
             */
            bool IsFileAlreadyProcessed(string fileName)
            {
                CheckProcessFile();
                //Vérifier si le fichier a déjà été traité en lisant la liste des fichiers traités
                string[] processedFiles = File.ReadAllLines(processedFilesPath);
                Console.WriteLine($"le ficher à déja été traité? {processedFiles.Contains(fileName)}");
                return processedFiles.Contains(fileName);
            }

            /**
             * Ajoute le nom du fichier à la liste des fichiers traités
             */
            void AddFileAsProcessed(string fileName)
            {
                CheckProcessFile();
                //Ajouter le nom du fichier à la liste des fichiers traités
                File.AppendAllText(processedFilesPath, fileName + Environment.NewLine);
            }

            /**
             * Valide la structure d'un fichier JSON
             */
            bool ValidateJson(string filePath)
            {
                string jsonString = File.ReadAllText(filePath);

                try
                {
                    Console.WriteLine("vérification de " + filePath);
                    JsonConvert.DeserializeObject(jsonString);
                    Console.WriteLine("Le fichier JSON est valide.");
                    return true;
                }
                catch (JsonReaderException ex)
                {
                    Console.WriteLine("Le fichier JSON est invalide : " + ex.Message);
                    return false;
                }

            }

            /**
             * Vérifie les champs obligatoires du fichier json sérialisé en objet c#
             */
            void ValidateInputData(JsonModel jsonObject)
            {
                if (string.IsNullOrEmpty(jsonObject.codeClient))
                {
                    throw new ArgumentException("Le code client ne peut pas être vide.");
                }
                if (string.IsNullOrEmpty(jsonObject.dateCommande))
                {
                    throw new ArgumentException("La date de commande ne peut pas être vide.");
                }
                if (string.IsNullOrEmpty(jsonObject.dateLivraison))
                {
                    throw new ArgumentException("La date de livraison ne peut pas être vide.");
                }
                if (jsonObject.lignes == null || jsonObject.lignes.Count == 0)
                {
                    throw new ArgumentException("Le bon de commande doit avoir au moins une ligne.");
                }
            }
        }
    }
}