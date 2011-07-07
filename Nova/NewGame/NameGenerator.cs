#region Copyright Notice
// ============================================================================
// Copyright (C) 2008 Ken Reed
// Copyright (C) 2009, 2010 stars-nova
//
// This file is part of Stars-Nova.
// See <http://sourceforge.net/projects/stars-nova/>.
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License version 2 as
// published by the Free Software Foundation.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>
// ===========================================================================
#endregion

#region Module Description
// ===========================================================================
// Randomly pick a star name from a list of star names and remove that name so
// that it doesn't get allocated again. This is really just a case of putting a
// fixed set of names into a hat and pulling them out one by one.
// ===========================================================================
#endregion

namespace Nova.NewGame
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class NameGenerator
    {
        private readonly Random randomGenerator = new Random();
        private readonly List<string> starNamePool = new List<string>();
        private readonly List<string> raceNamePool = new List<string>();
        private int counter;

        #region Construction

        /// <summary>
        /// Initializes a new instance of the ShipDesignDialog class.
        /// <para>Put all of our star names into our hat.</para>
        /// </summary>
        public NameGenerator()
        {
            this.starNamePool.AddRange(this.starNames);
            this.raceNamePool.AddRange(this.raceNames);
            this.counter = 0;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Randomly pull a star name out of our hat.
        /// </summary>
        public string NextStarName
        {
            get
            {
                int index = this.randomGenerator.Next(0, this.starNamePool.Count - 1);
                string name = starNamePool[index];

                this.starNamePool.RemoveAt(index);

                return name;
            }
        }
        
        /// <summary>
        /// Randomly pull a race name out of our hat.
        /// </summary>
        public string NextRaceName
        {
            get
            {
                if (this.raceNamePool.Count > 1)
                {
                    int index = this.randomGenerator.Next(0, this.raceNamePool.Count - 1);
                    string name = raceNamePool[index];
    
                    this.raceNamePool.RemoveAt(index);
    
                    return name;
                }
                else
                {
                    // none left. eek! - fall back to just adding a number
                    return raceNamePool[0] + counter++;
                }
            }
        }

        /// <summary>
        /// Return the number of star names we can generate.
        /// </summary>
        public int StarCapacity
        {
            get
            {
                return this.starNamePool.Count;
            }
        }
        
        public int RaceCapacity
        {
            get
            {
                return this.raceNamePool.Count;
            }
        }
  
        /// <summary>
        /// The list of race names we can return. Taken from Stars!.exe. May need changing?
        /// </summary>
        private readonly string[] raceNames = 
        {
            "Berserker",
            "Bulushi",
            "Golem",
            "Nulon",
            "Tritizoid",
            "Valadiac",
            "Ubert",
            "Felite",
            "Ferret",
            "House",
            "Cat",
            "Crusher",
            "Picardi",
            "Rush'n",
            "American",
            "Hawk",
            "Eagle",
            "Mensoid",
            "Loraxoid",
            "Hicardi",
            "Nairnian",
            "Cleaver",
            "Hooveron",
            "Nee",
            "Kurkonian"
        };
              
        
        /// <summary>
        /// The list of star names we can return.
        /// </summary>
        private readonly string[] starNames = 
        {
            "A'po",
            "Abacus",
            "Abbott",
            "Abdera",
            "Abel",
            "Abrams",
            "Accord",
            "Achernar",
            "Acid",
            "Acrux",
            "Acubens",
            "Adams",
            "Adhafera",
            "Adhara",
            "Afterthought",
            "Agena",
            "Ajar",
            "Albali",
            "Albemuth",
            "Albireo",
            "Alchiba",
            "Alcoa",
            "Alcor",
            "Alcyone",
            "Aldebaran",
            "Alderamin",
            "Alexander",
            "Alfirk",
            "Algedi",
            "Algedi",
            "Algenib",
            "Algieba",
            "Algol",
            "Algorab",
            "Alhena",
            "Alioth",
            "Alkaid",
            "Alkalurops",
            "Alkes",
            "All Work",
            "Allegro",
            "Allen",
            "Allenby",
            "Almach",
            "Almagest",
            "Almighty",
            "Alnasl",
            "Alnilam",
            "Alnitak",
            "Alpha Centauri",
            "Alphard",
            "Alphecca",
            "Alpheratz",
            "Alrescha",
            "Alsea",
            "Alshain",
            "Altair",
            "Altais",
            "Alterf",
            "Aludra",
            "Alula",
            "Aluminum",
            "Alya",
            "America",
            "Amontillado",
            "Ancha",
            "Andante",
            "Andromeda",
            "Angst",
            "Ankaa",
            "Anthrax",
            "Apple",
            "Applegate",
            "April",
            "Aqua",
            "Aquarius",
            "Arafat",
            "Arcade",
            "Arctic",
            "Arcturius",
            "Arcturus",
            "Argon",
            "Ariel",
            "Aries",
            "Arkab",
            "Armonk",
            "Armstrong",
            "Arneb",
            "Arnold",
            "Arpeggio",
            "Ascella",
            "Asellus",
            "Asellus",
            "Asgard",
            "Aspidiske",
            "Astair",
            "Asterope",
            "Atik",
            "Atlas",
            "Atria",
            "Atropos",
            "Auriga",
            "Aurora",
            "Australis",
            "Autumn Leaves",
            "Avior",
            "Awk",
            "Axelrod",
            "Azha",
            "Bach",
            "Baggy",
            "Bagnose",
            "Baker",
            "Bakwele",
            "Balder",
            "Baldrick",
            "Ball Bearing",
            "Bambi",
            "Bar None",
            "Barbecue",
            "Barrow",
            "Barry",
            "Bart",
            "Basil",
            "Basket Case",
            "Baten",
            "Bath",
            "Beacon",
            "Beauregard",
            "Beautiful",
            "Becrux",
            "Bed Rock",
            "Beethoven",
            "Beid",
            "Bellatrix",
            "Benetnasch",
            "Bentley",
            "Berry",
            "Beta",
            "Betelgeuse",
            "Betelgeuse",
            "Bfe",
            "Bidpai",
            "Biham",
            "Bilbo",
            "Bilskirnir",
            "Birthmark",
            "Black Hole",
            "Bladderworld",
            "Blinken",
            "Bloop",
            "Blossom",
            "Blue Ball",
            "Blue Dwarf",
            "Blue Giant",
            "Blush",
            "Bob",
            "Boca Raton",
            "Boethius",
            "Bog",
            "Bonaparte",
            "Bone",
            "Bones",
            "Bonn",
            "Bonus",
            "Boolean",
            "Bootes",
            "Borealis",
            "Borges",
            "Boron",
            "Bountiful",
            "Braddock",
            "Bradley",
            "Brass Rat",
            "Brin",
            "Bruski",
            "Buckshot",
            "Bufu",
            "Burgoyne",
            "Burrito",
            "Burrow",
            "Bush",
            "Buttercup",
            "Caelum",
            "Cain",
            "Calcium",
            "Callipus",
            "Cambridge",
            "Cancer",
            "Candide",
            "Candy Corn",
            "Canis Major",
            "Canis Minor",
            "Canopus",
            "Canterbury",
            "Capella",
            "Caph",
            "Capricornus",
            "Captain Jack",
            "Carbon",
            "Carcassonne",
            "Caroli",
            "Carter",
            "Carver",
            "Cassiopeia",
            "Castle",
            "Castor",
            "Cathlamet",
            "Catnip",
            "Cebalrai",
            "Celaeno",
            "Celery",
            "Celt",
            "Centaurus",
            "Cepheus",
            "Cerberus",
            "Ceres",
            "Cern",
            "Cetus",
            "Challenger",
            "Chamber",
            "Chandrasekhar",
            "Chaos",
            "Chara",
            "Charity",
            "Cheleb",
            "Chennault",
            "Cherry",
            "Chertan",
            "Cherub",
            "Chinese Finger",
            "Chlorine",
            "Chopin",
            "Chort",
            "Chubs",
            "Chunk",
            "Cinnamon",
            "Cirrus",
            "Clapton",
            "Clark",
            "Clatsop",
            "Clausewitz",
            "Clay",
            "Climax",
            "Clinton",
            "Clotho",
            "Clover",
            "Cochise",
            "Coda",
            "Code",
            "Columbia",
            "Columbus",
            "Continental",
            "Contra",
            "Coolidge",
            "Cootie",
            "Copper",
            "Cor",
            "Core",
            "Corner",
            "Cornhusk",
            "Cornwallis",
            "Corvus",
            "Cosine",
            "Costello",
            "Cotton Candy",
            "Cougar",
            "County Seat",
            "Cousin Louie",
            "Covenant",
            "Coyote Corners",
            "Crabby",
            "Cramp",
            "Crazy Horse",
            "Crisp X",
            "Croce",
            "Crow",
            "Crux",
            "Curley",
            "Cursa",
            "Custer",
            "Cygnus",
            "Dabih",
            "Dachshund",
            "Daily",
            "Daisy",
            "Dalmatian",
            "Darien",
            "Dark Planet",
            "Data",
            "Dave",
            "Dawn",
            "Dayan",
            "Deacon",
            "Decatur",
            "Deep Thought",
            "Defect",
            "Delta Delta Delta",
            "Delta",
            "Demski",
            "Deneb",
            "Deneb",
            "Denebola",
            "Denikin",
            "Denon",
            "Desert",
            "Desolate",
            "Devo",
            "Devon IV",
            "Dewey",
            "Dharma",
            "Diablo",
            "Diamond",
            "Diddley",
            "Dill Weed",
            "Dimna",
            "Dimple",
            "Dingleberry",
            "Dingly Dell",
            "Dinky",
            "Diphda",
            "Dipstick",
            "Discovery",
            "Distopia",
            "Ditto",
            "Dive",
            "Dnoces",
            "Do Re Mi",
            "Dog House",
            "Dollar",
            "Doodles",
            "Doris",
            "Double Tall Skinny",
            "Dowding",
            "Down And Out",
            "Draco",
            "Draft",
            "Dry Spell",
            "Dschubba",
            "Dubhe",
            "Dunsany",
            "Dwarte",
            "Dyson",
            "Early",
            "Earth",
            "Edasich",
            "Eden",
            "Eisenhower",
            "Elased",
            "Elder",
            "Electra",
            "Elephant Ear",
            "Elnath",
            "Elron",
            "Elsinore",
            "Emerald",
            "Emoclew",
            "Emperium Gate",
            "Empty",
            "Endeavor",
            "Enif",
            "Eno",
            "Enterprise",
            "Epsilon",
            "Equuleus",
            "Erasmus",
            "Eridanus",
            "Errai",
            "Escalator",
            "Esher",
            "Estes",
            "Esther",
            "Evergreen",
            "Excel",
            "Faith",
            "False Hopes",
            "Farragut",
            "Fez",
            "Finale",
            "Finger",
            "Fizbin",
            "Flaming Poodle",
            "Flapjack",
            "Fleabite",
            "Flint Stone",
            "Floyd",
            "Fluffy",
            "Fluorine",
            "Flutter Valve",
            "Foamytap",
            "Foch",
            "Foggy Bottom",
            "Fomalhaut",
            "Foresight",
            "Forest",
            "Forget-Me-Not",
            "Forgotten",
            "Forrest",
            "Forward",
            "Foucault's World",
            "Fox Trot",
            "Franklin",
            "Frederick",
            "Frost",
            "Fubar",
            "Fugue",
            "Furud",
            "Gacrux",
            "Gaia 2",
            "Galbraith",
            "Gamma",
            "Gangtok",
            "Garcia",
            "Garfunkel",
            "Gargantua",
            "Garlic",
            "Garnet",
            "Garnet",
            "Garp",
            "Gas",
            "Gasp",
            "Gates",
            "Gaye",
            "Gemini",
            "Genesis",
            "Genoa",
            "Geronimo",
            "Gertrude",
            "Giausar",
            "Gienah",
            "Gienah",
            "Girtab",
            "Gladiolus",
            "Gladsheim",
            "Glenn",
            "Globular Rex",
            "Godel",
            "Gold",
            "Gollum",
            "Gomeisa",
            "Goober",
            "Goofy",
            "Gorby",
            "Gordon",
            "Gornic",
            "Gout",
            "Graceland",
            "Graffias",
            "Grant",
            "Grape",
            "Grappo",
            "Graz",
            "Green House",
            "Greenbaum",
            "Greene",
            "Grendel",
            "Grep",
            "Grey Matter",
            "Grim Reaper",
            "Grouse",
            "Grumium",
            "Guano",
            "Guderian",
            "Gueneviere",
            "Gunk",
            "H2O",
            "H2SO4",
            "Hacker",
            "Hadar",
            "Haig",
            "Hal",
            "Halsey",
            "Hamal",
            "Hammer",
            "Happy",
            "Hard Ball",
            "Harding",
            "Harlequin",
            "Harris",
            "Harrison",
            "Hawking's Gut",
            "Hay Seed",
            "Heart",
            "Heatwave",
            "Heaven",
            "Hedtke",
            "Helium",
            "Hell",
            "Henbane",
            "Hercules",
            "Hermit",
            "Heroin",
            "Hexnut",
            "Hiho",
            "Hill",
            "Himshaw",
            "Hindsight",
            "Ho Hum",
            "Hodel",
            "Hoe",
            "Hoho",
            "Hollywood",
            "Homam",
            "Homer",
            "Homogeneous",
            "Hoof And Mouth",
            "Hoover",
            "Hope",
            "Horselover Fat",
            "Hot Tip",
            "Hoth",
            "Houdini",
            "Howe",
            "Hoze-O-Rama",
            "Huckleberry",
            "Hull",
            "Hummingbird",
            "Humus",
            "Hunt",
            "Hurl",
            "Hydra",
            "Hydrogen",
            "Hydroplane",
            "Hyperbole",
            "Icarus",
            "Ice Patch",
            "Iceball",
            "Indy",
            "Inferno",
            "Infinity Junction",
            "Innie",
            "Insane",
            "Inside-Out",
            "Invisible",
            "Io",
            "Iodine",
            "Izar",
            "Jerilyn",
            "Jersey",
            "Joffre",
            "Johnson",
            "Jones",
            "Jubitz",
            "June",
            "Juniper",
            "Jupiter",
            "K9",
            "Kaa",
            "Kaitos",
            "Kaitos",
            "Kalamazoo",
            "Kalila",
            "Kan",
            "Kant",
            "Kappa",
            "Karhide",
            "Kaus",
            "Kearny",
            "Keid",
            "Kennedy",
            "Kensplace",
            "Kent",
            "Kentaurus",
            "Kepler",
            "Kernel",
            "Kha Karpo",
            "Kidney",
            "King",
            "Kirk",
            "Kirkland",
            "Kitalpha",
            "Kitaro",
            "Kitchener",
            "Kiwi",
            "Klaupaucius",
            "Kline",
            "Kludge",
            "Knife",
            "Knob",
            "Kochab",
            "Kornephoros",
            "Kornilov",
            "Kosciusko",
            "Kruger",
            "Kulu",
            "Kumquat",
            "Kutuzov",
            "Kwaidan",
            "LGM 1",
            "LGM 2",
            "LGM 3",
            "LGM 4",
            "LSD",
            "La Te Da",
            "Lachesis",
            "Lafayette",
            "Lambda",
            "Larry",
            "Last Chance",
            "Latte",
            "Lavacious",
            "Lawrence",
            "Lazy B",
            "Le Petit Jean",
            "Lead Pants",
            "Lead",
            "Leaky Pipe",
            "Lee",
            "Lekgolo",
            "Lemnitzer",
            "Leo Minor",
            "Leo",
            "Leonardo",
            "Lesath",
            "Lever",
            "Leviathan",
            "Lhasa",
            "Libra",
            "Limbo",
            "Lincoln",
            "Lingo",
            "Linq",
            "Lisa",
            "Lithium",
            "Little Brother",
            "Little Sister",
            "Liver",
            "Lizard's Beak",
            "Loan Shark",
            "Logic",
            "Loki",
            "Longstreet",
            "Lopsided",
            "Love",
            "Lube Job",
            "Luigi",
            "Lukundoo",
            "Luscious",
            "Lycra",
            "Lynx",
            "Lyra",
            "M16",
            "MacArthur",
            "Macintosh",
            "Macrohard",
            "Magellan",
            "Maggie",
            "Maia",
            "Mallard",
            "Mamie",
            "Mana",
            "Mandelbrot",
            "Mandrake",
            "Maple Syrup",
            "Marfik",
            "Marge",
            "Marion",
            "Markab",
            "Marla",
            "Marlborough",
            "Mars",
            "Marshall",
            "Matar",
            "Match",
            "Mathilda",
            "Maude",
            "May",
            "Mayberry",
            "McBride",
            "McCartney",
            "McClellan",
            "McFarquardt",
            "McIntyre",
            "Me",
            "Meade",
            "Mebsuta",
            "Media",
            "Media",
            "Medusa",
            "Megrez",
            "Meissa",
            "Mekbuda",
            "Melthorne",
            "Memmon",
            "Memos",
            "Menkab",
            "Menkalinan",
            "Menkar",
            "Menkent",
            "Merak",
            "Mercury",
            "Meridianalis",
            "Merope",
            "Mesarthim",
            "Miaplacidus",
            "Midgard",
            "Midnight",
            "Milky Way",
            "Mimosa",
            "Mira",
            "Mirach",
            "Mirfak",
            "Mirror",
            "Mirzam",
            "Misery",
            "Mitchell",
            "Mizar",
            "Mobius",
            "Mocha",
            "Moe",
            "Mohlodi",
            "Molalla",
            "Moltke",
            "Molybdenum",
            "Money",
            "Mongo",
            "Montcalm",
            "Montgomery",
            "Moonbeam",
            "Morgan",
            "Moscow",
            "Mothallah",
            "Mother",
            "Mountbatten",
            "Mozart",
            "Mu",
            "Muhlifain",
            "Muliphen",
            "Mundus",
            "Mungle",
            "Muphrid",
            "Murat",
            "Muscida",
            "Muskrat",
            "Muspell",
            "Mustang",
            "Myopus",
            "Nada",
            "Nadir",
            "Naledi",
            "Naos",
            "Narnia",
            "Nashira",
            "Nastrond",
            "Navi",
            "Nawk",
            "Nebulae",
            "Neil",
            "Nekkar",
            "Nelson",
            "Neon",
            "Neptune",
            "Nerd",
            "Nessus",
            "Nether Region",
            "Neuronos",
            "Neuter",
            "Never Never Land",
            "New Kalapuya",
            "New",
            "Ney",
            "Nickel",
            "Niflheim",
            "Nifty",
            "Nihal",
            "Nimitz",
            "Nipso",
            "Nirvana",
            "Nitrogen",
            "Nivenyrral",
            "No Exit",
            "No Play",
            "No Respect",
            "No Return",
            "No Vacancy",
            "Noble Impulse",
            "Nod",
            "Nope",
            "Norm",
            "Notlob",
            "Notorious",
            "Nova",
            "Novelty",
            "Nowhere",
            "Nu",
            "Nunki",
            "Nusakan",
            "Nylon",
            "Oasis",
            "Oh Ho Ho",
            "Old",
            "Ollie",
            "Olympia",
            "Omega",
            "Oop Be Gone",
            "Opus 10",
            "Orange",
            "Orbison",
            "Oregano",
            "Orion",
            "Oshun",
            "Outie",
            "Oxygen",
            "Ozone",
            "PCP",
            "Panacea",
            "Pansy",
            "Pantagruel",
            "Paradise",
            "Parsley",
            "Patton",
            "Peacock",
            "Pearl",
            "Peat Moss",
            "Peekaboo",
            "Pegasus",
            "Penance",
            "Penzance",
            "Perry",
            "Perseus",
            "Pershing",
            "Pervo",
            "Petra",
            "Phact",
            "Phad",
            "Phaeton",
            "Phecda",
            "Pherkad",
            "Pheson",
            "Phi",
            "Phicol",
            "Philistia",
            "Pi",
            "PiR2",
            "Pickett",
            "Pickles",
            "Pilgrim's Harbor",
            "Pin Prick",
            "Pinball",
            "Pirate",
            "Pisces",
            "Pitstop",
            "Planet 10",
            "Planet 9",
            "Planet X",
            "Pleione",
            "Pluto",
            "Polaris",
            "Pollux",
            "Poly Gone",
            "Poly Siren",
            "Pop",
            "Porrima",
            "Posterior",
            "Potassium",
            "Pound",
            "PreVious",
            "Presley",
            "Prior",
            "Procyon",
            "Propus",
            "Provo",
            "Prude",
            "Prune",
            "Prydun",
            "Puberty",
            "Puddn'head",
            "Pulcherrima",
            "Puma",
            "Purgatory",
            "Puss Puss",
            "Putty",
            "Pyxidis",
            "Qaphqa",
            "Quark",
            "Quarter",
            "Quiche",
            "Quick Lick",
            "Quixote",
            "Radian",
            "Radish",
            "Radium",
            "Raisa",
            "Raisin",
            "Rake",
            "Ras",
            "Rasalgethi",
            "Rasalhague",
            "Rastaban",
            "Raster",
            "Raven's Eye",
            "Reagan",
            "Recalc",
            "Red Ball",
            "Red Dwarf",
            "Red Giant",
            "Red Storm",
            "Redemption",
            "Redmond",
            "Reedhome",
            "Register",
            "Regor",
            "Regulus",
            "Relight",
            "Replica",
            "Resistor",
            "Resort",
            "Revelation",
            "Rex",
            "Rhenium",
            "Rho",
            "Ricketts",
            "Rickover",
            "Rigel",
            "Right",
            "Rigil",
            "Rigil",
            "Ripper Jack",
            "Rock",
            "Rockette",
            "Rodney",
            "Rogers",
            "Romeo",
            "Rommel",
            "Roosevelt",
            "Rotanev",
            "Rough Shod",
            "Rubber",
            "Ruby",
            "Ruchba",
            "Ruchbah",
            "Rukbat",
            "Rundstedt",
            "Rutebaga",
            "Rye",
            "Saada",
            "Sabik",
            "Sad",
            "Sadalachbia",
            "Sadalmelik",
            "Sadalsuud",
            "Sadatoni",
            "Saddam",
            "Sadie",
            "Sadr",
            "Sagittarius",
            "Saiph",
            "Salamander",
            "Salsa",
            "Sam",
            "Same Here",
            "Samsonov",
            "Sand Castle",
            "Sands Of Time",
            "Sapphire",
            "Sargas",
            "Sartre",
            "Saturn",
            "Scandahoovia",
            "Scarlett",
            "Scat",
            "Scheat",
            "Schedar",
            "Scheherezade",
            "Schubert",
            "Schwiiing",
            "Scorpius",
            "Scotch",
            "Scott",
            "Scotts Valley",
            "Scotty",
            "Scud",
            "Scurvy",
            "Sea Squared",
            "Sed",
            "Seginus",
            "Selenium",
            "Senility",
            "Sequim 3",
            "Serapa",
            "Shaft",
            "Shaggy Dog",
            "Shangri-La",
            "Shank",
            "Shannon",
            "Shanty",
            "Shaula",
            "Sheliak",
            "Shelty",
            "Sheratan",
            "Sheridan",
            "Sherman",
            "Shinola",
            "Ship Shape",
            "Shoe Shine",
            "Shrine",
            "Siberia",
            "Sigma",
            "Silicon",
            "Silver",
            "Simon",
            "Simple",
            "Siren",
            "Sirius",
            "Situla",
            "Skat",
            "Skid Row",
            "Skidmark",
            "Skink",
            "Skloot",
            "Skunk",
            "Skynyrd",
            "Slag",
            "Slick",
            "Slime",
            "Slinky",
            "Sludge",
            "Smithers",
            "Smorgasbord",
            "Snack",
            "Snafu",
            "Snake's Belly",
            "Sniffles",
            "Snots",
            "Snuffles",
            "Sodium",
            "Sol",
            "Spaatz",
            "Spaceball",
            "Sparta",
            "Spay",
            "Spearmint",
            "Speed Bump",
            "Sphairos",
            "Sphere",
            "Spica",
            "Spitfire",
            "Spittle",
            "Split",
            "Springfield",
            "Spruance",
            "Spuds",
            "Sputnik",
            "Squidcakes",
            "Staff",
            "Stamp",
            "Stanley",
            "Status",
            "Steeple",
            "Stellar",
            "Steppenwolf",
            "Sterope",
            "Stilton",
            "Stilwell",
            "Sting",
            "Stinky Socks",
            "Stonehenge",
            "Stove Top",
            "Strange",
            "Strauss",
            "Strike 3",
            "Stuart",
            "Sualocin",
            "Suhail",
            "Sulafat",
            "Sulfur",
            "Sutra",
            "Swizzle Stick",
            "Syrma",
            "Taco",
            "Talitha",
            "Talking Desert",
            "Tangent",
            "Tania",
            "Tanj",
            "Tank Top",
            "Tao",
            "Tarazed",
            "Tartaruga",
            "Taton",
            "Tattoo",
            "Taurus",
            "Taygeta",
            "Tchaikovsky",
            "Teela",
            "Tegmine",
            "Telly",
            "Terrace",
            "Texas",
            "Thomas",
            "Thuban",
            "Thyme",
            "Tierra",
            "Tiger's Tail",
            "Timbuktu",
            "Timoshenko",
            "Tirpitz",
            "Tlon",
            "Tongue",
            "Toroid",
            "Tough Luck",
            "Trial",
            "Triffid",
            "Trismegistus",
            "Trog",
            "Truck Stop",
            "True Faith",
            "Truman",
            "Trurl",
            "Tsagigla'lal",
            "Tull",
            "Turing's World",
            "Tweedledee",
            "Tweedledum",
            "Twelfth Man",
            "Tycho B",
            "Ultima Thule",
            "Underdog",
            "Unukalhai",
            "Upsilon",
            "Uqbar",
            "Uranus",
            "Ursa Good",
            "Ursa Major",
            "Ursa Minor",
            "Utgard",
            "Utopia",
            "Vacancy",
            "Vacant",
            "Valhalla",
            "Vanilla",
            "Vega",
            "Vega",
            "Venus",
            "Verdi",
            "Veritas",
            "Vindemiatrix",
            "Virgin",
            "Virgo",
            "Viscous",
            "Vista",
            "Vivaldi",
            "Vox",
            "Waco",
            "Wagner",
            "Wainwright",
            "Walla Walla",
            "Wallaby",
            "Wallace",
            "Wammalammadingdong",
            "Wasat",
            "Washington",
            "Waterfall",
            "Wavell",
            "Wazn",
            "Weed",
            "Wellington",
            "Wendy",
            "Wezen",
            "Where",
            "Whiskey",
            "Whistler's Mother",
            "Who",
            "Wilbury",
            "Wingnut",
            "Winken",
            "Winkle",
            "Winky-Blinky",
            "Winter",
            "Witter",
            "Wizzle",
            "Wobbly",
            "Wobegon",
            "Wood Shed",
            "Woody",
            "Woozle",
            "Worm",
            "Wrench",
            "Wrist Rocket",
            "Wumpus",
            "X-Lacks",
            "X-Ray",
            "Xenon",
            "Y-Has",
            "Ya Betcha",
            "Yank",
            "Yeager",
            "Yed",
            "Yes",
            "Yildun",
            "Yoruba",
            "Yuppie Puppy",
            "Zahir",
            "Zaniah",
            "Zanzibar",
            "Zappa",
            "Zarquon",
            "Zaurak",
            "Zavijava",
            "Zebra",
            "Zed",
            "Zeppelin",
            "Zero",
            "Zeta",
            "Zhukovi",
            "Ziggurat",
            "Zippy",
            "Zosma",
            "Zucchini",
            "Zulu"
        };
        #endregion
    }
}
