using System;
using System.Collections.Generic;
using System.Text;

namespace PoseidonLogic
{
    public class NavigationNode
    {
        public string idfwbonavigation { get; set; }
        public string setnavigationcontextonclick { get; set; }
        public string numfwmarketgroups { get; set; }
        public string numbonavigationchildren { get; set; }
        public string name { get; set; }
        public List<string> idfwbonavigationtypes { get; set; }
        public List<NavigationNode> bonavigationnodes { get; set; }
        public List<MarketGroup> marketgroups { get; set; }
        public int defaultOrder { get; set; }
        public int nummarkets { get; set; }
        public int numevents { get; set; }
        public List<object> bonavigationnodelinks { get; set; }
        public List<object> gamegroups { get; set; }
        public string imageicon { get; set; }
    }

    public class EventGroup
    {
        public string name { get; set; }
        public string idfwmarketgroup { get; set; }
        public string idfwmarketgrouptype { get; set; }
        public List<PEvent> events { get; set; }
    }

    public class PEvent
    {
        public decimal idfoevent { get; set; }
        public string participantname_away { get; set; }
        public string participantname_home { get; set; }
        public string idfosport { get; set; }
        public string idfosporttype { get; set; }
        public string sporttypename { get; set; }
        public bool istradable { get; set; }
        public string name { get; set; }
        public double tsstart { get; set; }
        public List<PMarket> markets { get; set; }
        public int defaultOrder { get; set; }
        public string listcode { get; set; }
        public string externalprovider { get; set; }
        public string externalreference { get; set; }
        public string idfoeventtype { get; set; }
        public string sportName { get; set; }
        public string venue { get; set; }
        public DateTime Start { get; set; }
    }

    public class MarketGroup
    {
        public string name { get; set; }
        public string idfwmarketgroup { get; set; }
        public string idfwmarketgrouptype { get; set; }
        public List<PMarket> markets { get; set; }
        public int defaultOrder { get; set; }
    }

    public class PMarket
    {
        public string csvavailablebettypes { get; set; }
        public List<Pricetype> pricetypes { get; set; }
        public decimal idfoevent { get; set; }
        public string eventname { get; set; }
        public string idfosport { get; set; }
        public string idfosporttype { get; set; }
        public string sportname { get; set; }
        public decimal idfomarket { get; set; }
        public string name { get; set; }
        public decimal idfomarkettype { get; set; }
        public string idfoselectionorder { get; set; }
        public string participantname_away { get; set; }
        public string participantname_home { get; set; }
        public bool is4inrunning { get; set; }
        public bool ismainoutright { get; set; }
        public bool isplaceonlyoptionon { get; set; }
        public bool istradable { get; set; }
        public bool istrapbettingoptionon { get; set; }
        public object tsstart { get; set; }
        public string mtag { get; set; }
        public List<PSelection> selections { get; set; }
        public List<object> eachwayterms { get; set; }
        public List<object> mediacoverages { get; set; }
        public int defaultOrder { get; set; }
        public string idefmarkettype { get; set; }
        public object tsbetstart { get; set; }
        public object tsbetend { get; set; }
        public string externalprovider { get; set; }
        public string externalreference { get; set; }
        public int eventMarketCount { get; set; }
        public bool ismainline { get; set; }
        public string idfomarketlinetype { get; set; }
        public bool ismatchhandicaptype { get; set; }
        public bool isunderover { get; set; }
        public int internalOrder { get; set; }
    }

    public class PSelection
    {
        public string currentpricedown { get; set; }
        public string currentpriceup { get; set; }
        public string hadvalue { get; set; }
        public string idfomarket { get; set; }
        public string idfoselection { get; set; }
        public bool is1stfavourite { get; set; }
        public bool is2ndfavourite { get; set; }
        public string name { get; set; }
        public string shortname { get; set; }
        public string idfobolifestate { get; set; }
        public string selectionhashcode { get; set; }
        public int defaultOrder { get; set; }
        public string externalprovider { get; set; }
        public string externalreference { get; set; }

        public string listcode { get; set; }
    }

    public class Pricetype
    {
        public string idfopricetypeclass { get; set; }
        public string value { get; set; }
        public int defaultOrder { get; set; }
    }

    public class EventGroupModel
    {        
        public string GroupId { get; set; }

        public string Sport { get; set; }

        public string Tournament { get; set; }

        public string GroupName { get; set; }

        public bool IsSubscribed { get; set; }

        public int Version { get; set; }
     
        public HashSet<decimal> eventIds = new HashSet<decimal>();
    }
}
