using System.Collections.Generic;

public class JsonModel
{
    public string E { get; set; }
    public string idEntete { get; set; }
    public string numCommandeEdi { get; set; }
    public string numFact { get; set; }
    public string codeTerminal { get; set; }
    public int type { get; set; }
    public string codeResponsablesecteur { get; set; }
    public string dateCommande { get; set; }
    public string dateLivraison { get; set; }
    public string codeClient { get; set; }
    public string refExtClient { get; set; }
    public string numSouche { get; set; }
    public string numTarif { get; set; }
    public int remise { get; set; }
    public int codeDepot { get; set; }
    public double totalHT { get; set; }
    public double totalTTC { get; set; }
    public string commentaireEntete { get; set; }
    public int nbLignes { get; set; }
    public int totalQuantiteUc { get; set; }
    public int totalQuantiteUd { get; set; }
    public string glnClient { get; set; }
    public string glnEmetteur { get; set; }
    public string glnLivreur { get; set; }
    public string refExtCommande { get; set; }
    public string refExtBL { get; set; }
    public string refExtFacture { get; set; }
    public string refExtRS { get; set; }
    public string dateEcheance { get; set; }
    public string createdAt { get; set; }
    //contient toutes les lignes du documents
    public IList<Lignes> lignes { get; set; }
    //public IList<undefined> paiement { get; set; }
}

public class Lignes
{
    public string L { get; set; }
    public int numLigne { get; set; }
    public string idligne { get; set; }
    public string identete { get; set; }
    public string codeArticle { get; set; }
    public IList<string> refExtArticle { get; set; }
    public string eanUc { get; set; }
    public string eanUd { get; set; }
    public int pcb { get; set; }
    public int quantiteUc { get; set; }
    public int quantiteUd { get; set; }
    public int quantiteGratuiteUc { get; set; }
    public int quantiteGratuiteUd { get; set; }
    public double puBrutHtUc { get; set; }
    public double puBrutHtUd { get; set; }
    public string puNetHtUc { get; set; }
    public double puNetHtUd { get; set; }
    public double tauxTva { get; set; }
    public double puBrutTtcUc { get; set; }
    public double puBrutTtcUd { get; set; }
    public double puNetTtcUc { get; set; }
    public double puNetTtcUd { get; set; }
    public double? remiseEntetePourcent { get; set; }
    public double remiseLignePourcent { get; set; }
    public double remiseLigneMontant { get; set; }
    public double remisePromoEntete { get; set; }
    public double remisePromoLigne { get; set; }
    public double remisePromoLogistique { get; set; }
    public double remiseCalculee { get; set; }
    public string commentaireLigne { get; set; }
    public int? idPromotion { get; set; }
    public int quantiteCommandeeUc { get; set; }
    public int quantiteCommandeeUd { get; set; }
    public int? quantitePrepareeUc { get; set; }
    public int? quantitePrepareeUd { get; set; }
    public int? quantiteRetourneeUc { get; set; }
    public int quantiteRetourneeUd { get; set; }
    public string created_at { get; set; }
    public string remisePDA { get; set; }
    public string poidsBrutUc { get; set; }
}