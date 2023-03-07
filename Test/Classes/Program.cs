using System;
using Newtonsoft.Json;
using System.IO;
using System.Net;
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
        //Chemin du fichier pour stocker les noms de fichiers traités
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

            //Objet métier: ouverture des bdd  
           //base comptable 
            Objets100cLib.BSCPTAApplication100c dbCompta = new Objets100cLib.BSCPTAApplication100c();
            //base commerciale
            Objets100cLib.BSCIALApplication100c dbCommerce = new Objets100cLib.BSCIALApplication100c();

            //Paramètres pour se connecter aux bases
            ParamDb paramBaseCompta = new ParamDb(DBCOMPTAPATH, DBCOMPTAUSER, DBCOMPTAPWD);
            ParamDb paramBaseCial = new ParamDb(DBCIALPATH, DBCIALUSER, DBCIALPWD);

            //ouverture des bases de données
            Console.WriteLine("tentative de connexion a la base SAGE...");
            if (OpenDbComptable(dbCompta, paramBaseCompta) && (OpenDbCommercial(dbCommerce, paramBaseCial, dbCompta)))
            {
                Console.ReadLine();
                //Lecture du fichier json 

                //parcours des fichiers json dans le répertoire
                List<string> localFiles = Directory.GetFiles(LOCALFILEPATH).ToList();
                foreach (string s in localFiles)
                {
                    Console.WriteLine(s);   
                    if (!(IsFileAlreadyProcessed(s))) // vérifie si le fichier a deja été traité
                    {
                        // génère un objet JsonModel depuis le fichier json
                        Console.WriteLine("conversion de "+ s+ " en modele c#");
                        if (ValidateJson(s))
                        {
                            JsonModel jsonModel = DeserialiseJson(s);
                            //créer le bon de commande 
                            Createcmd(jsonModel);
                        }
                        //ajoute le nom du ficher dans la liste des fichiers traités
                        AddFileAsProcessed(s);
                    }     
                }
            }
            Console.ReadLine();
            CloseDB(dbCommerce, dbCompta);
            Console.ReadLine();

            /*********************************************************Méthodes***********************************************************/
            bool OpenDbComptable(Objets100cLib.BSCPTAApplication100c dbComptable, ParamDb paramCpta)
            {
                try
                {
                    //ouverture de la base comptable 
                    dbComptable.Name = paramCpta.getDbname();
                    dbComptable.Loggable.UserName = paramCpta.getuser();
                    dbComptable.Loggable.UserPwd = paramCpta.getpwd();

                    dbComptable.Open();
                    Console.WriteLine("succes connexion à " + paramCpta.getDbname());
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("erreur pour l'ouverture de la base comptable: " + e);
                    return false;
                }
            }

            bool OpenDbCommercial(Objets100cLib.BSCIALApplication100c dbCommercial, ParamDb paramCial, Objets100cLib.BSCPTAApplication100c bdcompta)
            {
                try
                {
                    //ouverture de la base comptable 
                    dbCommercial.Name = paramCial.getDbname();
                    dbCommercial.Loggable.UserName = paramCial.getuser();
                    dbCommercial.Loggable.UserPwd = paramCial.getpwd();
                    dbCommercial.CptaApplication = bdcompta;

                    dbCommercial.Open();
                    Console.WriteLine("succes connexion à " + paramCial.getDbname());
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("erreur pour l'ouverture de la base commerciale: " + e);
                    return false;
                }
            }

            bool CloseDB(Objets100cLib.BSCIALApplication100c dbCommercial, Objets100cLib.BSCPTAApplication100c dbComptable)
            {
                try
                {
                    dbComptable.Close();
                    dbCommercial.Close();
                    Console.WriteLine("base fermée");
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return false;
                }
            }

            /**
             * Créer un bon de commande et l'insert dans la base de données
             */
            void Createcmd(JsonModel jsonObject)
            {
                //entete du bon de commande
                Console.WriteLine("Création d'un bon de commande...");
                Objets100cLib.IBODocumentVente3 entete = null;
                Objets100cLib.IBODocumentVenteLigne3 lignes = null;

                try
                {
                    entete = dbCommerce.FactoryDocumentVente.CreateType(Objets100cLib.DocumentType.DocumentTypeVenteCommande);
                    //affecte un client au bon de commande
                    entete.SetDefaultClient(dbCompta.FactoryClient.ReadNumero(jsonObject.codeClient));
                    entete.DO_Date = StringToDate(jsonObject.dateCommande, "yyyyMMddHHmmss"); // méthode de conversion
                    entete.DO_DateLivr = StringToDate(jsonObject.dateLivraison, "yyyyMMddHHmmss");
                    Objets100cLib.IBPSoucheVente souche = (Objets100cLib.IBPSoucheVente)dbCommerce.FactorySoucheVente.Create();
                    //attibuer la souche de type PDA
                    entete.Souche = (Objets100cLib.IBISouche)dbCommerce.FactorySoucheVente.ReadIntitule("PDA");
                    //Affecte le prochain numéro de pièce en fonction de la souche (chrono)
                    entete.SetDefaultDO_Piece();
                    //entete.DO_Ref = "UUUU";
                    //entete.DO_TotalHT = jsonObject.totalHT;
                    // entete.DO_TotalTTC = jsonObject.totalTTC;
                    entete.Write();
                    Console.WriteLine("entete du document crée!");

                    // lister les info libres
                    Console.WriteLine("liste des champs infos libres");
                    foreach (Objets100cLib.IBIField field in dbCommerce.FactoryDocument.InfoLibreFields)
                    {
                        Console.WriteLine("Intitulé : " + field.Name);
                    }
                    //insertion infos libres
                    entete.InfoLibre["IDBIZIIPAD"] = jsonObject.idEntete;
                    entete.Write();
                    //ajout des lignes (articles) dans le document 
                    //parcours de la collection de ligne du document json
                    Console.WriteLine("nb de lignes:" + jsonObject.lignes.Count);
                    foreach (Lignes l in jsonObject.lignes)
                    {
                        //ajout d'un article dans le document
                        lignes = (Objets100cLib.IBODocumentVenteLigne3)entete.FactoryDocumentLigne.Create();
                        Objets100cLib.IBOArticle3 article = dbCommerce.FactoryArticle.ReadReference(l.codeArticle);
                        Console.WriteLine("tentative d'insértion de :" + l.codeArticle + "qte:" + l.quantiteUc);
                        lignes.SetDefaultArticle(article, l.quantiteUc);              
                        lignes.Write();
                    }
                    Console.WriteLine("articles ajoutés");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            void Createprocesscmd(JsonModel jsonObject)
            {
                //entete du bon de commande
                Console.WriteLine("Création d'un bon de commande...");
                Objets100cLib.IBODocumentVente3 entete = null;

                try
                {
                    
                    entete = dbCommerce.FactoryDocumentVente.CreateType(Objets100cLib.DocumentType.DocumentTypeVenteCommande);
                    Objets100cLib.IPMDocument mProcessDoc = (Objets100cLib.IPMDocument)dbCommerce.CreateProcess_Document(Objets100cLib.DocumentType.DocumentTypeVenteCommande);
                    entete = (Objets100cLib.IBODocumentVente3)mProcessDoc.Document;
                   
                    entete.SetDefaultClient(dbCompta.FactoryClient.ReadNumero(jsonObject.codeClient));

                    entete.DO_Date = StringToDate(jsonObject.dateCommande, "yyyyMMddHHmmss"); // méthode de conversion
                    entete.DO_DateLivr = StringToDate(jsonObject.dateLivraison, "yyyyMMddHHmmss");
                    Objets100cLib.IBPSoucheVente souche = (Objets100cLib.IBPSoucheVente)dbCommerce.FactorySoucheVente.Create();
                    //attibuer la souche de type PDA
                    entete.Souche = (Objets100cLib.IBISouche)dbCommerce.FactorySoucheVente.ReadIntitule("PDA");
                    //Affecte le prochain numéro de pièce en fonction de la souche (chrono)
                    entete.SetDefaultDO_Piece();
                   
                    Console.WriteLine("entete du document crée!");

                    // lister les info libres

                    Console.WriteLine("liste des champs infos libres");
                    foreach (Objets100cLib.IBIField field in dbCommerce.FactoryDocument.InfoLibreFields)
                    {
                        Console.WriteLine("Intitulé : " + field.Name);
                    }
                    //insertion infos libres
                    entete.InfoLibre["IDBIZIIPAD"] = jsonObject.idEntete;
                 

                    //ajout des lignes dans le document 
                    //parcours de la collection de ligne du document json
                    Console.WriteLine("nb de lignes:" + jsonObject.lignes.Count);
                    foreach (Lignes l in jsonObject.lignes)
                    {
                        //ajout d'un article dans le document
                       
                        mProcessDoc.AddArticle(dbCommerce.FactoryArticle.ReadReference(l.codeArticle), l.quantiteUc);
                        Console.WriteLine("tentative d'insértion de :" + l.codeArticle + "qte:" + l.quantiteUc);
                        
                    }
                    //lignes.Article.AR_Ref = "08G1DANA";
                    // lignes.Write();
                    mProcessDoc.Process();
                    Console.WriteLine("articles ajoutés");

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
           
            /**
             * Converti un fichier json en objet C#
             */
            JsonModel DeserialiseJson(string path)
            {
                try
                {
                    Console.WriteLine("Lecture du fichier Json:" + path);
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
             * Converti une chaine de caractère en Date au format choisi
             */
            DateTime StringToDate(string dateString, String format)
            {
                DateTime date;
                if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {
                    return date;
                }
                else
                {
                    throw new ArgumentException("La chaîne fournie n'est pas au format attendu", nameof(dateString));
                }
            }

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

            //Vérifie si le fichier a déjà été traité en vérifiant s'il figure dans la liste des fichiers traités
            bool IsFileAlreadyProcessed(string fileName)
            {
                CheckProcessFile();
                // Vérifier si le fichier a déjà été traité en lisant la liste des fichiers traités
                string[] processedFiles = File.ReadAllLines(processedFilesPath);
                Console.WriteLine(processedFiles.Contains(fileName));
                return processedFiles.Contains(fileName);
            }

            //Ajoute le nom du fichier à la liste des fichiers traités
            void AddFileAsProcessed(string fileName)
            {
                CheckProcessFile();
                //Ajouter le nom du fichier à la liste des fichiers traités
                File.AppendAllText(processedFilesPath, fileName + Environment.NewLine);
            }

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
        }
    }
}