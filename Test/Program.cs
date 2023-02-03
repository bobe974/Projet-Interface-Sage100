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

            /*******************************************************Objet métier *********************************************/
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

              //ouverture d'une base 
              /**A la place de la propriété Name, il est également possible d’utiliser les propriétés CompanyServer
                  et CompanyDatabaseName pour affecter respectivement le serveur/instance SQL et la base de 
                  données SQL**/
            Objets100cLib.BSCPTAApplication100c bdCompta = new Objets100cLib.BSCPTAApplication100c();
            Objets100cLib.BSCIALApplication100c bdCommercial = new Objets100cLib.BSCIALApplication100c();
            bdCompta.Name = @"C:\Users\Utilisateur\Desktop\Projet 1\test\STOCKSERVICE.mae";
            
            try {
                bdCompta.Loggable.UserName = "<Administrateur>";
                bdCompta.Loggable.UserPwd = "AR2003";
                bdCompta.Open();
                Console.WriteLine("Succes");
               
               
            }catch(Exception e){
                Console.WriteLine(e);
            }
            
            Console.ReadLine();

            //ajouter un enregistrement dans la base
            Objets100cLib.IPMDocument processDoc = bdCommercial.CreateProcess_Document(Objets100cLib.DocumentType.DocumentTypeVenteLivraison);
            Objets100cLib.IBODocument3 doc = (Objets100cLib.IBODocument3) processDoc.Document;
        }    
        
            

}
}
