# 🤖 Project Name: LLM alapú ZH-generáló és értékelő rendszer

Ez a README összefoglalja a backend működési logikáját és az elérhető API végpontokat, különös tekintettel a **Gemini** (Google LLM) integrációra, mely az automatikus tesztgenerálásért és kódkiértékelésért felel.

***

## 1. ⚙️ Teszt Kezelés (TestController)

Ez a modul kezeli a ZH (Zárthelyi) feladatok regisztrációját, legyen az automatikusan generált, vagy manuálisan (fájlból) feltöltött.

### 1.1. Automatikus ZH Generálás (Oktatói Funkció)

| Végpont | Metódus | Cél |
| :--- | :--- | :--- |
| `/api/test/generate` | **POST** | Új programozási ZH feladat generálása a Gemini LLM segítségével. |

**Kulcs Logika:**

1.  **Bemenet:** A kérés fogadja a `TestParametersDto`-t (témakör, programozási nyelv, max pontszám).
2.  **Prompt Előkészítés:** A Service lekéri a '`programozas_zh_generalo`' nevű prompt sablont a `prompt_sablonok` adatbázis táblából.
3.  **LLM Hívás:** A rendszer elküldi a promptot a **`gemini-2.5-flash-lite`** modellnek. A válasz fogadása **szigorú JSON formátumban** történik.
4.  **Mentés:** A kapott JSON válasz (tartalmazza a '`leiras`' és '`cim`' mezőket) regisztrálásra kerül a `zh` táblába.

### 1.2. Külső Teszt Feltöltése

| Végpont | Metódus | Cél |
| :--- | :--- | :--- |
| `/api/test/upload` | **POST** | Külső, fájl alapú ZH leírás feltöltése és regisztrálása a rendszerben. |

**Kulcs Logika:**

1.  **Bemenet:** `ExternalTestUploadDto` (tartalmazza az ZH leírást `IFormFile`-ként és a metaadatokat JSON formátumban).
2.  **Mentés:** A ZH metaadatok feldolgozása, majd regisztrálása a `zh` táblába (az automatikus generálással egyenrangú entitásként).

***

## 2. 📝 Megoldás és Kiértékelés (CorrectionController)

Ez a modul kezeli a hallgatói megoldások beküldését és az automatikus, LLM-alapú kiértékelést.

### 2.1. Megoldás Beküldése (Hallgatói Funkció)

| Végpont | Metódus | Cél |
| :--- | :--- | :--- |
| `/api/correction/submit` | **POST** | Hallgatói programkód (megoldás) mentése. |

**Kulcs Logika:**

1.  **Bemenet:** `SolutionSubmitDto` (Neptun kód, ZH azonosító, beküldött kód).
2.  **Mentés:** Létrehozásra kerül egy `FeltoltottMegoldas` entitás, mely mentésre kerül a `feltoltott_megoldasok` táblába.
3.  **Állapot:** A '`pont`' és '`ertekeles`' mezők a mentéskor **NULL** értéket kapnak.

### 2.2. Automatikus Kiértékelés (LLM Javítás)

| Végpont | Metódus | Cél |
| :--- | :--- | :--- |
| `/api/correction/start` | **POST** | Egy adott beküldött megoldás kijavíttatása a Gemini LLM-mel. |

**Kulcs Logika:**

1.  **Bemenet:** `CorrectionParamsDto` (a kiértékelendő `feltoltesId`, a `JavitasFokusz` és a `PontozasiRendszer`).
2.  **Adatgyűjtés:** A Service lekéri a hallgatói kódot **és** a hozzá tartozó ZH feladat leírását az adatbázisból.
3.  **Komplex Prompt:** A javítási sablon + ZH leírás + Hallgatói kód + Javítási fókusz adatokból összeáll egy **precíz LLM utasítás**.
4.  **LLM Hívás:** A prompt elküldése a Gemini modellnek. A modell **szigorú JSON válaszban** küldi vissza a '`Pont`' és '`Ertekeles`' mezőket.
5.  **Adatbázis Frissítés:** A `feltoltott_megoldasok` tábla megfelelő rekordja frissül az LLM által adott '`Pont`' és '`Ertekeles`' értékekkel.

***

## 3. 📄 Sablon Kezelés (PromptController)

Ez a modul felelős a Gemini LLM által használt belső prompt sablonok karbantartásáért, így a rendszer logikája anélkül finomítható, hogy a kódot módosítani kellene.

| Végpont | Metódus | Cél |
| :--- | :--- | :--- |
| `/api/prompt/{name}` | **GET** | Egy adott sablon lekérdezése. |
| `/api/prompt` | **PUT** | Meglévő sablon frissítése. |
| `/api/prompt/{name}` | **DELETE** | Sablon törlése. |

**Kulcs Logika:**

1.  **Adatbázis Kapcsolat:** Közvetlen adathozzáférés a `prompt_sablonok` táblához (`PromptRepository`) a lekérdezés és frissítés érdekében.
2.  **Rendszer Függőség:** A `TestController` és `CorrectionController` Service-ei minden LLM hívás előtt az itt beállított, aktuális sablonokat használják.