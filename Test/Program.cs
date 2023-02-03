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
                Console.WriteLine("succes connection");
                Console.ReadLine();
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
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("erreur pour l'ouverture de la base commerciale: " + e);
                    return false;
                }
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
