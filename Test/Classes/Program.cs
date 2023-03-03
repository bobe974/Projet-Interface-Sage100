using System;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using Test.Classes;
using System.Collections.Generic;
using System.Globalization;

namespace Test

{
    class Program
    {

        static void Main(string[] args)
        {
            /*******************************************Connexion et récupération des json par FTP  ****************************/

            // TODO: 2 solution: 1-> télécharger les fichiers json ou les lire directement sur le serveur, vérifier les données et insertion


            /******************************************************* Lecture du fichier json  **********************************/
            //chemin du fichier json
            string jsonPath = @"C:\Users\Utilisateur\Desktop\Projet 1\fichier json\BiZiiPAD_20221129_80881.json";
            //string jsonPath = @"C:\Users\Utilisateur\Desktop\Projet 1\fichier json\BiZiiPAD_20221014_79394.json";
            //string jsonPath = @"C:\Users\Utilisateur\Desktop\Projet 1\fichier json\BiZiiPAD_20221129_80882.json";

            // génère un objet JsonModel depuis le fichier json
            JsonModel jsonObj = DeserialiseJson(jsonPath);

            //Console.WriteLine(jsonObj.lignes[0].codeArticle);
            //Console.WriteLine(jsonObj.lignes[1].codeArticle);
            Console.ReadLine();

            // test d'affichage des lignes
            Console.WriteLine("test d'affichage des lignes du json");
            IList<Lignes> lesLignes = jsonObj.lignes;
            Console.WriteLine("nb de lignes:" + lesLignes.Count);
            foreach (Lignes l in lesLignes)
            {
                Console.WriteLine("num ligne:" + l.numLigne);
                Console.WriteLine("id:" + l.idligne);
                Console.WriteLine("identete:" + l.identete);
                Console.WriteLine("code article: " + l.codeArticle);
            }

            Console.ReadLine();

            /*******************************************************Objet métier: ouverture d'une bdd  **********************************/
            //base comptable 
            Objets100cLib.BSCPTAApplication100c dbCompta = new Objets100cLib.BSCPTAApplication100c();
            //base commerciale
            Objets100cLib.BSCIALApplication100c dbCommerce = new Objets100cLib.BSCIALApplication100c();

            //Paramètres pour se connecter aux bases

            ParamDb paramBaseCompta = new ParamDb(@"C:\Users\Utilisateur\Desktop\Projet 1\test\STOCKSERVICE.mae", "<Administrateur>", "AR2003");
            ParamDb paramBaseCial = new ParamDb(@"C:\Users\Utilisateur\Desktop\Projet 1\test\STOCKSERVICE.gcm", "<Administrateur>", "AR2003");

            //ouverture des bases de données
            Console.WriteLine("tentative de connexion a la base SAGE...");
            if (OpenDbComptable(dbCompta, paramBaseCompta) && (OpenDbCommercial(dbCommerce, paramBaseCial, dbCompta)))
            {

                Console.ReadLine();
                Createcmd(jsonObj);
                //Createprocesscmd(jsonObj);
            }

            CloseDB(dbCommerce, dbCompta);
            Console.ReadLine();

            /****************************************************Insertion avec objets métiers*******************************************/

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
                Objets100cLib.IBODocument3 info = null;

                try
                {
                    // entete = (Objets100cLib.IBODocumentVente3)dbCommerce.FactoryDocumentVente.Create();
                    entete = dbCommerce.FactoryDocumentVente.CreateType(Objets100cLib.DocumentType.DocumentTypeVenteCommande);

                    entete.SetDefaultClient(dbCompta.FactoryClient.ReadNumero(jsonObject.codeClient));

                    //entete.Client.CT_Num = "TEST2";
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

                    //ajout des lignes dans le document 
                    //parcours de la collection de ligne du document json
                    Console.WriteLine("nb de lignes:" + lesLignes.Count);
                    foreach (Lignes l in jsonObj.lignes)
                    {
                        //ajout d'un article dans le document
                        lignes = (Objets100cLib.IBODocumentVenteLigne3)entete.FactoryDocumentLigne.Create();
                        Objets100cLib.IBOArticle3 article = dbCommerce.FactoryArticle.ReadReference(l.codeArticle);

                        Console.WriteLine("tentative d'insértion de :" + l.codeArticle + "qte:" + l.quantiteUc);
                        lignes.SetDefaultArticle(article, l.quantiteUc);              
                        lignes.Write();
                    }
                    //lignes.Article.AR_Ref = "08G1DANA";
                   // lignes.Write();
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
                    Console.WriteLine("nb de lignes:" + lesLignes.Count);
                    foreach (Lignes l in jsonObj.lignes)
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
            void Readdata(Objets100cLib.BSCIALApplication100c dbcommerce)
            {
                /*******************test affichage enregistrement d'article ******************************/
                Objets100cLib.IBICollection collection = null;
                //Objets100cLib.IBOArticle3 article = null;
                collection = dbcommerce.FactoryArticle.List;
                //collection = dbCommerce.FactoryDocumentLigne.List;

                Console.WriteLine("nombre d'objet dans la collection:" + collection.Count);
                foreach (Objets100cLib.IBOArticle3 unArticle in collection)
                //foreach (Objets100cLib.IBODocumentLigne3 unArticle in collection)
                {
                    Console.WriteLine(unArticle.AR_CodeBarre);
                    Console.WriteLine(unArticle.AR_Design);

                    //Console.WriteLine(unArticle.DateCreation);
                    //Console.WriteLine(unArticle.Collaborateur); 
                }

                String article = dbcommerce.FactoryArticle.ReadReference("08G1DANA").AR_CodeBarre;
                Console.WriteLine("article: " + article);
                Console.WriteLine("article: " + dbcommerce.FactoryArticle.ReadCodeBarre("0000030043992").AR_DateCreation);
                Console.ReadLine();
                dbcommerce.FactoryDocumentLigne.ReadLigne(734124);
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

            
            /**
             * Etabli un connexion FTP  puis télécharge plusieurs fichiers suivant la liste donnée en paramètre (
             */
            void DownloadMultipleFilesFromFtp(String user, String pwd, String url, List<string> listFilesPath, string localFolderPath)
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
            void DownloadFileFromFtp(FtpWebRequest request, Stream responseStream, string fileUrl, string destPath)
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
}