using System;
using System.Globalization;

namespace Test.Classes
{
    class SageCommandeManager
    {
        //base comptable 
        private Objets100cLib.BSCPTAApplication100c dbCompta;
        //base commerciale
        private Objets100cLib.BSCIALApplication100c dbCommerce;
        public bool isconnnected = false;

        public SageCommandeManager(ParamDb paramCompta, ParamDb paramCommercial)
        {
            //initialisation et connexion aux bases sage100
            this.dbCompta = new Objets100cLib.BSCPTAApplication100c();
            this.dbCommerce = new Objets100cLib.BSCIALApplication100c();
            if (OpenDbComptable(dbCompta, paramCompta) && (OpenDbCommercial(dbCommerce, paramCommercial, dbCompta)))
            {
                isconnnected = true;
            }
        }

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

        public bool CloseDB()
        {
            try
            {
                this.dbCompta.Close();
                this.dbCommerce.Close();
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
        public void Createcmd(JsonModel jsonObject)
        {
            //entete du bon de commande
            Console.WriteLine("Création d'un bon de commande...");
            Objets100cLib.IBODocumentVente3 entete = null;
            Objets100cLib.IBODocumentVenteLigne3 lignes = null;

            // Vérification des contraintes de cohérence
            DateTime dateCommande = StringToDate(jsonObject.dateCommande, "yyyyMMddHHmmss");
            DateTime dateLivraison = StringToDate(jsonObject.dateLivraison, "yyyyMMddHHmmss");

            if (dateLivraison < dateCommande)
            {
                throw new ArgumentException("La date de livraison doit être postérieure à la date de commande");
            }

            // Vérification de la validité des données
            if (!dbCompta.FactoryClient.ExistNumero(jsonObject.codeClient))
            {
                throw new ArgumentException("Le code client n'existe pas dans la base de données Sage");
            }

            //ouvre une transaction pour insérer le bon de commande dans la base de données
            using (var transaction = new System.Transactions.TransactionScope())
            {
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
                    //Commit la transaction
                    transaction.Complete();
                    Console.WriteLine("articles ajoutés");
                }
                catch (Exception e)
                {
                    Console.WriteLine("erreur pour la création du bon de commande: " + e);
                }
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
    }
}
