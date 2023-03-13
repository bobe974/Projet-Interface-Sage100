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
        private string dbname = null;
        public bool isconnected = false;


        public string[,] souchesStockservice = new string[3, 18]
        {
            {"BO", "STS21", "STS22", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""},
            {"GM", "STS01", "STS02", "STS03", "STS04", "STS05", "STS06", "WEB01", "STS07", "STS08", "STS30", "STS31", "STS32", "STS34", "STS35", "FLUID", "RUNMARKET", "EDI"},
            {"IMP", "STS11", "STS12", "STS13", "STS14", "STS22", "STS33", "", "", "", "", "", "", "", "", "", "", ""}
        };

        public string[,] souchesDisbp = new string[3, 19]
        {
            {"BO", "DSP21", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""},
            {"GM", "DSP01", "DSP02", "DSP03", "DSP04", "DSP05", "DSP06", "WEB01", "DSP07", "DSP08", "DSP30", "DSP32", "DSP98", "DSP34", "DSP35", "FLUID", "RUNMARKET", "DSP31", "EDI"},
            {"IM", "DSP11", "DSP12", "DSP13", "DSP14", "DSP22", "DSP33", "", "", "", "", "", "", "", "", "", "", "",""}
        };

        public string[,] souchesRedisma = new string[3, 19]
        {
             {"BO", "RED21", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "", "",""},
             {"GM", "RED01", "RED02", "RED03", "RED04", "RED05", "RED06", "WEB01", "RED07", "RED08", "RED30", "RED32", "RED98", "RED34", "RED35", "FLUID", "RUNMARKET", "RED31", "EDI"},
             {"IM", "RED11", "RED12", "RED13", "RED14", "RED22", "RED33", "", "", "", "", "", "", "", "", "", "", "",""}
        };

        public SageCommandeManager(ParamDb paramCompta, ParamDb paramCommercial)
        {
            //initialisation et connexion aux bases sage100
            this.dbCompta = new Objets100cLib.BSCPTAApplication100c();
            this.dbCommerce = new Objets100cLib.BSCIALApplication100c();
            //récupere le nom de la base pour faire des traitements personnalisés
            this.dbname = paramCommercial.getName();
            if (OpenDbComptable(dbCompta, paramCompta) && (OpenDbCommercial(dbCommerce, paramCommercial, dbCompta)))
            {
                isconnected = true;
            }
        }
        bool OpenDbComptable(Objets100cLib.BSCPTAApplication100c dbComptable, ParamDb paramCpta)
        {
            try
            {
                //ouverture de la base comptable 
                paramCpta.getName();
                dbComptable.Name = paramCpta.getDbPath();
                dbComptable.Loggable.UserName = paramCpta.getuser();
                dbComptable.Loggable.UserPwd = paramCpta.getpwd();

                dbComptable.Open();
                Console.WriteLine("succes connexion à " + paramCpta.getDbPath());
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
                dbCommercial.Name = paramCial.getDbPath();
                dbCommercial.Loggable.UserName = paramCial.getuser();
                dbCommercial.Loggable.UserPwd = paramCial.getpwd();
                dbCommercial.CptaApplication = bdcompta;
                dbCommercial.Open();
                Console.WriteLine("succes connexion à " + paramCial.getDbPath());
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
        public bool Createcmd(JsonModel jsonObject)
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
                throw new ArgumentException("La date de livraison doit être postérieur à la date de commande");
            }

            //Vérification de la validité des données
            if (!dbCompta.FactoryClient.ExistNumero(jsonObject.codeClient))
            {
                throw new ArgumentException("Le code client n'existe pas dans la base de données Sage");
            }

            //vérification des articles
            foreach (Lignes l in jsonObject.lignes)
            {
                if (!dbCommerce.FactoryArticle.ExistReference(l.codeArticle))
                {
                    throw new ArgumentException($"Le code article {l.codeArticle} n'existe pas dans la base de données Sage");
                }
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
                   
                    //TODO vérifier que la souche existe avant insertion
                    String nomsouche = getSouche(jsonObject.codeTerminal, getTabCorrespondance(dbname));
                    Console.WriteLine("nom de la souche: " + nomsouche);
                    if (dbCommerce.FactorySoucheVente.ExistIntitule(nomsouche)){

                        Objets100cLib.IBPSoucheVente souche = (Objets100cLib.IBPSoucheVente)dbCommerce.FactorySoucheVente.Create();
                        //attibuer la souche 
                        entete.Souche = (Objets100cLib.IBISouche)dbCommerce.FactorySoucheVente.ReadIntitule(nomsouche);
                    }
                    else
                    {
                        Console.WriteLine($"La souche {nomsouche} ne correspond à auncune souche dans SAGE! Base actuelle: {dbname}");
                    }
                    //Affecte le prochain numéro de pièce en fonction de la souche (chrono)
                    entete.SetDefaultDO_Piece();
                    entete.DO_Ref = jsonObject.codeTerminal + jsonObject.idEntete;
                    //entete.DO_TotalHT = jsonObject.totalHT;
                    // entete.DO_TotalTTC = jsonObject.totalTTC;
                    entete.Write();
                    Console.WriteLine("entete du document crée!");

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
                        Console.WriteLine("tentative d'insertion de l'article: " + l.codeArticle + " qte: " + l.quantiteUc);
                        lignes.SetDefaultArticle(article, l.quantiteUc);
                        lignes.Write();
                    }
                    //Commit la transaction
                    transaction.Complete();
                    Console.WriteLine("Articles ajoutés\n");
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Erreur pour la création du bon de commande: " + e);
                    return false;
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
                    Console.WriteLine("tentative d'insertion de l'article :" + l.codeArticle + " qte:" + l.quantiteUc);

                }
                //lignes.Article.AR_Ref = "08G1DANA";
                // lignes.Write();
                mProcessDoc.Process();
                Console.WriteLine("");
                Console.WriteLine("articles ajoutés\n");
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
                throw new ArgumentException("La date n'est pas au format attendu", nameof(dateString));

            }
        }

        /**
         * retourne la souche correspondant au code terminal
         */
        public String getSouche(String codeTerminal, string[,] tabCorrespondance)
        {
            string souche = null;
            for (int i = 0; i < tabCorrespondance.GetLength(0); i++){

                for(int j =0; j< tabCorrespondance.GetLength(1); j++)
                {
                   // Console.WriteLine("i: " + i + "j: "+ j + " "+ tabCorrespondance[i,j]);
                    if(codeTerminal == tabCorrespondance[i, j])
                    {
                        //les souches  sont a l'index 0 de chaque tableau
                        souche = tabCorrespondance[i, 0];
                        //Console.WriteLine(codeTerminal + "trouvé a la position: " + i + j);
                        //Console.WriteLine(codeTerminal + "respond donc à la souche" + tabCorrespondance[i, 0]);
                    }
                }
            }
            return souche;
        }
        /**
         * 
         */
        string[,] getTabCorrespondance(String dbname)
        {
            switch (dbname)
            {
                case "STOCKSERVICE":
                    Console.WriteLine("base STOCKSERVICE");
                    return souchesStockservice;
                    break;
                case "DISBEP":
                    Console.WriteLine("base DISBEP");
                    return souchesStockservice;
                    break;
                case "REDISMA":
                    Console.WriteLine("base REDISMA");
                    return souchesRedisma;
                    break;
                default:
                    Console.WriteLine("Nom de fichier non reconnu.");
                    return null;
                    break;
            }

        }

    }
}
