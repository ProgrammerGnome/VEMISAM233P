-- =========================================
-- Tisztítás (Táblák eldobása, ha léteznek)
-- =========================================

DROP TABLE IF EXISTS feltoltott_megoldasok CASCADE;
DROP TABLE IF EXISTS zh CASCADE;
DROP TABLE IF EXISTS hallgato CASCADE;
DROP TABLE IF EXISTS prompt_sablonok CASCADE;

-- =========================================
-- 1. Tábla: Hallgato (Student)
-- =========================================

CREATE TABLE hallgato (
    neptun_kod VARCHAR(6) PRIMARY KEY,
    nev VARCHAR(255) NOT NULL
);


-- =========================================
-- 2. Tábla: ZH (Zárthelyi / Teszt)
-- =========================================

CREATE TABLE zh (
    zh_id SERIAL PRIMARY KEY,
    targy_id VARCHAR(50) NOT NULL, -- Tantárgy azonosító (külső rendszerből)
    cim VARCHAR(255) NOT NULL,
    leiras TEXT,                   -- A feladat szövege/leírása (Gemini által generált)
    prog_nyelv VARCHAR(50),
    maxpont INTEGER NOT NULL CHECK (maxpont >= 0),
    mintamegoldas TEXT,
    pontozasi_szempontok TEXT
);


-- =========================================
-- 3. Tábla: Feltoltott_megoldasok (UploadedSolutions)
-- =========================================

CREATE TABLE feltoltott_megoldasok (
    feltoltes_id SERIAL PRIMARY KEY,
    
    -- Idegen kulcs a ZH-ra (melyik feladathoz tartozik)
    zh_id INTEGER NOT NULL,
    CONSTRAINT fk_feltoltott_zh
        FOREIGN KEY (zh_id) 
        REFERENCES zh(zh_id) 
        ON DELETE CASCADE, -- Ha a ZH törlődik, a megoldásai is törlődnek.

    -- Idegen kulcs a Hallgatóra (ki küldte be)
    hallgato_id VARCHAR(6) NOT NULL,
    CONSTRAINT fk_feltoltott_hallgato
        FOREIGN KEY (hallgato_id) 
        REFERENCES hallgato(neptun_kod) 
        ON DELETE RESTRICT, -- Nem engedi a hallgatót törölni, ha van megoldása.

    bekuldott_megoldas TEXT NOT NULL,
    pont INTEGER, -- Kiértékelt pontszám (nullázható, ha még nincs javítva)
    ertekeles TEXT -- A Gemini által adott részletes szöveges értékelés
);


-- =========================================
-- 4. Tábla: Prompt_sablonok (Prompt Templates)
-- =========================================

CREATE TABLE prompt_sablonok (
    id SERIAL PRIMARY KEY,
    sablon_nev VARCHAR(255) UNIQUE NOT NULL, -- Pl: 'programozas_zh_generalo'
    prompt_szoveg TEXT NOT NULL              -- A sablon szövege, placeholder-ekkel
);


-- =========================================
-- 5. Kezdő Adatok (Prompt Sablonok)
-- =========================================
-- Ezek a sablonok elengedhetetlenek a szoftver működéséhez!

INSERT INTO prompt_sablonok (sablon_nev, prompt_szoveg) VALUES
(
    'programozas_zh_generalo', 
    'Generálj egy programozási feladatot, amelynek fő témaköre: "{temakor}". A feladat típusa: "{feladat_tipus}". A feladatot a {prog_nyelv} nyelvhez igazítsd. Kérlek, szigorúan a következő JSON formátumban add meg a választ: { "cim": "...", "leiras": "...", "progNyelv": "{prog_nyelv}", "maxPont": {max_pont} }'
),
(
    'javitas_prompt_template', 
    'A feladatod egy hallgatói programkód kiértékelése. Szigorúan tartsd be a megadott javítási fókuszt és pontozási rendszert. A válaszodnak egy JSON objektumnak kell lennie, amely tartalmazza a Pont és Ertekeles mezőket.

    --- Javítási Paraméterek ---
    Javítási fókusz: {JavitasFokusz}
    Pontozási rendszer: {PontozasiRendszer}
    Maximális pont: {MaxPont}

    --- ZH Feladat Leírása ---
    {ZhLeiras}

    --- Hallgatói Megoldás ---
    Programozási nyelv: {ProgNyelv}
    {HallgatoiMegoldasKodja}

    Szigorúan a következő JSON formátumban adj választ: { "Pont": 0, "Ertekeles": "..." }'
),
(
    'poc_programozas_zh_generalo',
    'GENERÁLÁS:
    SYSTEM PROMPT:
    Te egy felsőoktatási oktatási asszisztens vagy, aki programozási kurzusokhoz automatikusan generál ZH (zárthelyi) feladatokat. 
    Feladatod: a megadott kurzusadatok (tárgytematika, mintafeladatok, témakörleírások) alapján új, a kurzushoz illeszkedő, értékelhető és kompetenciamérő ZH-feladatokat készíteni.

    A ZH-feladatok legyenek:
    - **tematikusan illeszkedők** a megadott témakörökhöz,
    - **formailag konzisztensen** felépítettek a mintafeladatok alapján,
    - **kompetencia-alapúak**, azaz konkrét tanulási kimenetet mérjenek,
    - **pontozhatók és egyértelműek** (minden feladathoz tartozzon pontozási séma és bemenet/kimenet példa, ha releváns),
    - **különböző nehézségűek**, ha a felhasználó ezt kéri.

    A válasz formátuma mindig JSON, az alábbi struktúrában:
    {
    "feladat_cime": "",
    "temakor": "",
    "leiras": "",
    "kompetenciak": [],
    "pontozas": "",
    "pelda_bemenet": "",
    "pelda_kimenet": "",
    "nehézségi_szint": ""
    }

    ---

    INPUT PARAMÉTEREK:

    [TÁRGYTEMATIKA]
    A kurzus fő témakörei és nyelvspecifikus elvárások.
    Példa:
    """
    - Vezérlési szerkezetek (if, switch, for, while)
    - Függvények és paraméterátadás
    - Osztályok és objektumok
    - Fájlkezelés
    - Programozási nyelv: C#
    """

    [MINTA ZÁRTHELYI FELADATOK]
    Korábban használt vagy manuálisan megadott ZH-feladatok, amelyek mintát adnak a formátumhoz és nehézségi szinthez.
    Példa:
    """
    Feladat: Írjon programot, amely beolvas egy számot, és kiírja, hogy páros-e!
    Pontozás: 5 pont
    Kompetencia: alap vezérlési szerkezetek
    """

    [TÉMAKÖRLEÍRÁSOK]
    Az egyes témakörökhöz tartozó tanulási célok és kompetenciák.
    Példa:
    """
    - Függvények: paraméterátadás, visszatérési érték, újrafelhasználhatóság
    - Osztályok: adattagok, metódusok, példányosítás, OOP alapelvek
    """

    [FELHASZNÁLÓI BEÁLLÍTÁSOK]
    - Feladatok száma: {feladat_db}
    - Néhézség: {nehezsegi_szint} (pl. "könnyű", "közepes", "nehéz")
    - Programozási nyelv: {nyelv}

    ---

    INSTRUKCIÓ:
    A fenti paraméterek alapján generálj {feladat_db} darab új ZH-feladatot, amelyek:
    1. Illeszkednek a [TÁRGYTEMATIKA] témaköreihez,  
    2. Követik a [MINTA ZÁRTHELYI FELADATOK] formátumát,  
    3. Lefedik a [TÉMAKÖRLEÍRÁSOK]-ban leírt kompetenciákat,  
    4. Megfelelnek a [FELHASZNÁLÓI BEÁLLÍTÁSOK]-ban megadott nehézségi szintnek és nyelvnek.

    A válasz JSON-tömb formájában adja vissza az összes feladatot:
    [
    {...},
    {...},
    {...}
    ]'
),
(
    'poc_javitas_prompt_template',
    'ÉRTÉKELÉS:
    SYSTEM PROMPT:
    Te egy felsőoktatási oktatási asszisztens vagy, aki programozási kurzusokhoz beadott ZH megoldásokat automatikusan értékel. 
    A célod: a hallgatói megoldásokat **részletesen, objektíven és kompetenciaalapon** értékelni a megadott pontozási szempontok és mintamegoldás alapján.

    Feladataid:
    1. Elemezd a megadott **ZH-feladatot**, hogy megértsd, mit kellett megoldani.
    2. Hasonlítsd össze a **hallgatói megoldást** a **mintamegoldással**.
    3. Alkalmazd a megadott **pontozási szempontokat** (részpontszámok, kritériumok).
    4. Adj részletes **értékelő szöveget** minden főbb szempont mellé.
    5. Számítsd ki az **összesített pontszámot** 

    A válasz formátuma mindig JSON, az alábbi struktúrában:
    {
    "feladat_cime": "",
    "osszpont": 0,
    "elert_pont": 0,
    "ertekeles_reszletes": [
        {
        "szempont": "",
        "max_pont": 0,
        "elert_pont": 0,
        "indoklas": ""
        }
    ],
    "osszegzo_ertekeles": "",
    }

    ---

    INPUT PARAMÉTEREK:

    [ZÁRTHELYI FELADAT]
    A hallgató által megoldandó feladat szövege.
    Példa:
    """
    Készítsen Python programot, amely beolvassa egy fájl tartalmát, majd megszámolja a sorok számát, és kiírja az eredményt.
    """

    [MINTAMEGOLDÁS]
    A tanár által megadott, helyes megoldás.  
    Példa:
    """
    with open("input.txt", "r") as f:
        lines = f.readlines()
    print("Sorok száma:", len(lines))
    """

    [PONTOZÁSI SZEMPONTOK]
    Az értékelés alapját képező objektív kritériumok és részpontszámok.
    Példa:
    """
    - Fájl megnyitása helyesen: 2 pont
    - Beolvasás ciklussal vagy beépített függvénnyel: 2 pont
    - Sorok megszámlálása: 2 pont
    - Eredmény helyes kiírása: 2 pont
    - Kódstílus / hibakezelés: 2 pont
    Összesen: 10 pont
    """

    [HALLGATÓ MEGOLDÁSA]
    A beadott programkód vagy válasz.
    Példa:
    """
    f = open("input.txt")
    print(len(f))
    """

    INSTRUKCIÓ:
    A fenti paraméterek alapján értékeld a [HALLGATÓ MEGOLDÁSÁT], a [ZÁRTHELYI FELADAT], a [MINTAMEGOLDÁS] és a [PONTOZÁSI SZEMPONTOK] figyelembevételével.  
    Add vissza a választ a JSON struktúrában, **minden szempontnál részletes indoklással**, majd egy összegző értékeléssel térjen vissza.'
);

-- Biztosítom, hogy a Hallgato tábla neptun_kod oszlopa ne legyen üres!
INSERT INTO hallgato (neptun_kod, nev) 
VALUES ('NEPTUN', 'Teszt Hallgato');
