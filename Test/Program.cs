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

            //création d'une base "comptabilité"
            /**  Objets100cLib.BSCPTAApplication100c e = new Objets100cLib.BSCPTAApplication100c();
              try{
                  e.Name = @"C:\test\test1.mae";
                  e.Create();
                  Console.WriteLine("base créer");
              }catch(Exception err){
                  Console.WriteLine("erreur:"+ err);
              }
              Console.ReadLine(); **/

            /*******************************************************Objet métier: ouverture d'une bdd  **********************************/
            //base comptable 
            Objets100cLib.BSCPTAApplication100c dbCompta = new Objets100cLib.BSCPTAApplication100c();
            //base commerciale
            Objets100cLib.BSCIALApplication100c dbCommerce = new Objets100cLib.BSCIALApplication100c();

            //Paramètres pour se connecter aux bases
            ParamDb paramBaseCompta = new ParamDb(@"C:\Users\Utilisateur\Desktop\Projet 1\test\STOCKSERVICE.mae", "<Administrateur>", "AR2003");
            ParamDb paramBaseCial = new ParamDb(@"C:\Users\Utilisateur\Desktop\Projet 1\test\STOCKSERVICE.gcm", "<Administrateur>", "AR2003");

            //ouverture des bases de données
            if (OpenDbCommercial(dbCommerce, paramBaseCial) && OpenDbComptable(dbCompta, paramBaseCompta)){

                //création d'un client
                Objets100cLib.IBOClient3 client = null;
                client = CreateClient(dbCompta, client);
               // Console.WriteLine("lecture d'un client...");
               
            //     client.FactoryClient.ReadNumero("011010003");
                Console.ReadLine();
                Console.WriteLine("afficher des données de la base StockServices...");
                Readdata(dbCommerce);
            }



            //ajouter un enregistrement dans la base
            //Objets100cLib.IPMDocument processDoc = bdCommercial.CreateProcess_Document(Objets100cLib.DocumentType.DocumentTypeVenteLivraison);
            //Objets100cLib.IBODocument3 doc = (Objets100cLib.IBODocument3) processDoc.Document;

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

             bool OpenDbCommercial(Objets100cLib.BSCIALApplication100c dbCommercial, ParamDb paramCial)
            {
                try
                {
                    //ouverture de la base comptable 
                    dbCommercial.Name = paramCial.getDbname();
                    dbCommercial.Loggable.UserName = paramCial.getuser();
                    dbCommercial.Loggable.UserPwd = paramCial.getpwd();

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

             Objets100cLib.IBOClient3 CreateClient(Objets100cLib.BSCPTAApplication100c bdComptable, Objets100cLib.IBOClient3 objClient)
            {
                try
                {
                    //objClient = (Objets100cLib.IBOClient3)bdComptable.FactoryClient.Create();
                    Objets100cLib.IBOClient3 client = null;
                    client = (Objets100cLib.IBOClient3)bdComptable.FactoryClient.Create();
                    try
                    {
                        //client.SetDefault();
                        //client.CT_Num = "TEST2";
                        //client.CT_Intitule = "youpi";
                        //client.Write();
                        //Console.WriteLine("client créer!");
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
