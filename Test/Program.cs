using System;
using System.Data.Odbc;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test

{
    class Program 
    {
        static void Main(string[] args)
        {

            /*******************************************************Objet métier: ouverture d'une bdd  **********************************/
            //base comptable 
            Objets100cLib.BSCPTAApplication100c dbCompta = new Objets100cLib.BSCPTAApplication100c();
            //base commerciale
            Objets100cLib.BSCIALApplication100c dbCommerce = new Objets100cLib.BSCIALApplication100c();

            //Paramètres pour se connecter aux bases
            ParamDb paramBaseCompta = new ParamDb(@"C:\Users\Utilisateur\Desktop\Projet 1\test\STOCKSERVICE.mae", "<Administrateur>", "AR2003");
            ParamDb paramBaseCial = new ParamDb(@"C:\Users\Utilisateur\Desktop\Projet 1\test\STOCKSERVICE.gcm", "<Administrateur>", "AR2003");

            //ouverture des bases de données
            if(OpenDbComptable(dbCompta, paramBaseCompta) && (OpenDbCommercial(dbCommerce, paramBaseCial, dbCompta))){

                //création d'un client
                //Objets100cLib.IBOClient3 client = null;
                //client = CreateClient(dbCompta, client);
               
              
                Console.ReadLine();
                //Console.WriteLine("afficher des données de la base StockServices...");
                //Readdata(dbCommerce);
                Console.WriteLine("création d'un bon de commande...");
                Createcmd();
                //Console.WriteLine(dbCompta.FactoryClient.ReadNumero("212060031")); 
                
                Console.ReadLine();
            }

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

             void Createcmd()
            {
               
                //entete du bon de commande
                Objets100cLib.IBODocumentVente3 entete = null;
                Objets100cLib.IBODocumentVenteLigne3 lignes = null;

                try
                {
                   // entete = (Objets100cLib.IBODocumentVente3)dbCommerce.FactoryDocumentVente.Create();
                    entete = dbCommerce.FactoryDocumentVente.CreateType(Objets100cLib.DocumentType.DocumentTypeVenteCommande);
                    entete.SetDefaultClient(dbCompta.FactoryClient.ReadNumero("TEST2"));
                    entete.DO_Date = DateTime.Now;
                    entete.SetDefaultDO_Piece();
                       
                    entete.Write();
                    Console.WriteLine("entete du document crée!");

                    //ajout des lignes dans le document 
                    lignes = (Objets100cLib.IBODocumentVenteLigne3)entete.FactoryDocumentLigne.Create();
                    
                    //attribution d'un article
                    lignes.SetDefaultArticle(dbCommerce.FactoryArticle.ReadReference("08G1DANA"), 1);
                    lignes.Write();
                    Console.WriteLine("article ajouté");
                }
                catch(Exception e) {

                    Console.WriteLine(e);
                }
            }
             Objets100cLib.IBOClient3 CreateClient(Objets100cLib.BSCPTAApplication100c bdComptable, Objets100cLib.IBOClient3 objClient)
            {
                try
                {
                    //objClient = (Objets100cLib.IBOClient3)bdComptable.FactoryClient.Create();
                    Objets100cLib.IBOClient3 client = null;
                    client = (Objets100cLib.IBOClient3)bdComptable.FactoryClient.Create();
                  
                    try
                    {
                        // Insertion d'un client
                        //client.SetDefault();
                        //client.CT_Num = "TEST2";
                        //client.CT_Intitule = "test";
                        //client.Write();
                        //Console.WriteLine("client crée!");
                      
                       
                     

                        return client;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return null;
                    }
                    //Objets100cLib.IBODocumentLigneAllFactory docligne = (Objets100cLib.IBODocumentLigneAllFactory)dbCommerce.FactoryDocumentLigne.Create();
                    //Objets100cLib.IBODocumentVenteFactory3 doc  = (Objets100cLib.IBODocumentVenteFactory3)dbCommerce.FactoryDocumentVente.Create();
                 
                    //création de l'entete
                    //Objets100cLib.IBODocumentVente3 docEntete = null;
                    //docEntete = dbCommerce.FactoryDocumentVente.CreateType(Objets100cLib.DocumentType.DocumentTypeVenteCommande);                     
                }
                catch(Exception e){
                    Console.WriteLine(e);
                    return null;
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
                foreach(Objets100cLib.IBOArticle3 unArticle in collection )
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
                /*******************************************************************************************/
            }
        }
    }


    public class ParamDb
    {
        private String dbName, user, pwd;

        public ParamDb(String dbName, String user, String pwd)
        {
            this.dbName = dbName;
            this.user = user;
            this.pwd = pwd;
        }

        public String getDbname()
        { return this.dbName; }

        public String getuser()
        { return this.user; }

        public String getpwd()
        { return this.pwd; }
    }

}
