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
    maxpont INTEGER NOT NULL CHECK (maxpont >= 0)
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
('programozas_zh_generalo', 
'Generálj egy programozási feladatot, amelynek fő témaköre: "{temakor}". A feladat típusa: "{feladat_tipus}". A feladatot a {prog_nyelv} nyelvhez igazítsd. Kérlek, szigorúan a következő JSON formátumban add meg a választ: { "cim": "...", "leiras": "...", "progNyelv": "{prog_nyelv}", "maxPont": {max_pont} }'
),
('javitas_prompt_template', 
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
);

-- Biztosítom, hogy a Hallgato tábla neptun_kod oszlopa ne legyen üres!
INSERT INTO hallgato (neptun_kod, nev) 
VALUES ('NEPTUN', 'Teszt Hallgato');
