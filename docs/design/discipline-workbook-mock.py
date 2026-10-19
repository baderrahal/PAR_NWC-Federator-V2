"""Mock of the per-discipline workbook, C06 ME, from the real run of 19 Oct 2026.
Claude's design for Bader to approve. Code copies this layout exactly in the C# writer."""
import json, re, math, sys
from openpyxl import Workbook
from openpyxl.styles import Font, PatternFill, Alignment, Border, Side
from openpyxl.utils import get_column_letter

SRC = sys.argv[1]
OUT = sys.argv[2]
data = json.load(open(SRC))["ME"]

# ---------- house look ----------
NAVY, CYAN, GREEN = "081931", "00AEE6", "50BF34"
RED, AMBER, GREY = "C62828", "F2A900", "8A94A3"
INK, MUTED = "1F2A37", "6B7785"
HEAD_FILL, BAND_FILL = "E8EDF4", "F6F8FB"
ISSUE_FILL, ISSUE_INK = "FDE2E1", "9B1C1C"
LOW_FILL, LOW_INK = "FFF4D6", "7A5300"
BODY, DEMI = "Franklin Gothic Book", "Franklin Gothic Demi"

def f(size=10, bold=False, color=INK, name=None, italic=False):
    return Font(name=name or (DEMI if bold else BODY), size=size, color=color, italic=italic)

def fill(hex_):
    return PatternFill("solid", start_color=hex_, end_color=hex_)

thin = Side(style="thin", color="D5DBE3")
cyan_line = Side(style="medium", color=CYAN)
ROW_BORDER = Border(bottom=thin)
HEAD_BORDER = Border(bottom=cyan_line)

STATUS = {  # word: (fill, ink)
    "FIX": (RED, "FFFFFF"), "MISSING": (RED, "FFFFFF"),
    "REVIEW": (AMBER, NAVY), "OK": (GREEN, "FFFFFF"), "NOT CHECKED": ("BFC5CC", NAVY),
}
PRIORITY = {"High": (RED, "FFFFFF"), "Medium": (AMBER, NAVY), "Low": ("D5DBE3", NAVY)}

def chip(cell, word, table=STATUS):
    bg, ink = table[word]
    cell.value = word
    cell.fill = fill(bg)
    cell.font = Font(name=DEMI, size=9, color=ink)
    cell.alignment = Alignment(horizontal="center", vertical="center")

def banner(ws, title, sub, last_col):
    ws.sheet_view.showGridLines = False
    ws.column_dimensions["A"].width = 2
    for c in range(1, last_col + 1):
        ws.cell(1, c).fill = fill(NAVY)
        ws.cell(2, c).fill = fill(NAVY)
        ws.cell(3, c).fill = fill(CYAN)
    ws.row_dimensions[1].height = 34
    ws.row_dimensions[2].height = 20
    ws.row_dimensions[3].height = 4
    ws["B1"].value = title
    ws["B1"].font = Font(name=DEMI, size=18, color="FFFFFF")
    ws["B1"].alignment = Alignment(vertical="center")
    ws["B2"].value = sub
    ws["B2"].font = Font(name=BODY, size=10, color=CYAN)
    ws["B2"].alignment = Alignment(vertical="center")

def header(ws, row, names, widths):
    for i, (n, w) in enumerate(zip(names, widths), start=2):
        c = ws.cell(row, i, n)
        c.font = Font(name=DEMI, size=10, color=NAVY)
        c.fill = fill(HEAD_FILL)
        c.border = HEAD_BORDER
        c.alignment = Alignment(vertical="center", wrap_text=True,
                                horizontal="left")
        ws.column_dimensions[get_column_letter(i)].width = w
    ws.row_dimensions[row].height = 30

def body(cell, value, fmt=None, align="left", ink=INK, band=False, wrap=False):
    cell.value = value
    cell.font = f(color=ink)
    cell.border = ROW_BORDER
    cell.alignment = Alignment(horizontal=align, vertical="center", wrap_text=wrap)
    if fmt:
        cell.number_format = fmt
    if band:
        cell.fill = fill(BAND_FILL)

def issue(cell, low=False):
    cell.fill = fill(LOW_FILL if low else ISSUE_FILL)
    cell.font = Font(name=DEMI, size=10, color=LOW_INK if low else ISSUE_INK)

def print_setup(ws, last_col, head_row):
    ws.page_setup.orientation = "landscape"
    ws.page_setup.paperSize = ws.PAPERSIZE_A3
    ws.page_setup.fitToWidth = 1
    ws.page_setup.fitToHeight = 0
    ws.sheet_properties.pageSetUpPr.fitToPage = True
    ws.print_title_rows = f"{head_row}:{head_row}"
    ws.oddFooter.left.text = "&A"
    ws.oddFooter.right.text = "Page &P of &N"
    ws.print_options.horizontalCentered = True

# ---------- data from the run ----------
short = lambda name: re.sub(r"^1104-PAR-[0-9A-Z]+-ZZZ-([A-Z]{2})-MOD-(\d+)\.nwc$", r"\1-\2", name)
bld = lambda name: name.split("-")[2]

ME_MODELS = [  # every ME NWC in Parent\C06, with what the run measured from its building's reference
    # name, compared with, metres off (None = under 1 m, "ref" = it is the reference)
    ("1104-PAR-1B06BC-ZZZ-ME-MOD-000001.nwc", "AR", None),
    ("1104-PAR-1B06G1-ZZZ-ME-MOD-000001.nwc", "AR", None),
    ("1104-PAR-1B06K1-ZZZ-ME-MOD-000001.nwc", "AR", None),
    ("1104-PAR-1B06M1-ZZZ-ME-MOD-000001.nwc", "ME", "ref"),
    ("1104-PAR-1B06P1-ZZZ-ME-MOD-000001.nwc", "AR", None),
    ("1104-PAR-1B06PE-ZZZ-ME-MOD-000001.nwc", "AR", 7.16),
    ("1104-PAR-1B06PH-ZZZ-ME-MOD-000001.nwc", "AR", 8.11),
    ("1104-PAR-1B06PH-ZZZ-ME-MOD-000002.nwc", "AR", 118.682),
    ("1104-PAR-1B06PK-ZZZ-ME-MOD-000004.nwc", "ME", "ref"),
    ("1104-PAR-1B06PO-ZZZ-ME-MOD-000001.nwc", "AR", 4.36),
    ("1104-PAR-1B06PP-ZZZ-ME-MOD-000001.nwc", "AR", None),
    ("1104-PAR-1B06PP-ZZZ-ME-MOD-000002.nwc", "AR", 255.238),
    ("1104-PAR-1B06PP-ZZZ-ME-MOD-000003.nwc", "AR", 328.335),
    ("1104-PAR-1B06WL-ZZZ-ME-MOD-000001.nwc", "EL", 30.215),
    ("1104-PAR-1B06WL-ZZZ-ME-MOD-000002.nwc", "EL", 30.215),
    ("1104-PAR-1B06WM-ZZZ-ME-MOD-000001.nwc", "AR", 1.21),
    ("1104-PAR-1B06WO-ZZZ-ME-MOD-000001.nwc", "AR", 1.85),
    ("1104-PAR-1C06M2-ZZZ-ME-MOD-000001.nwc", "ME", "ref"),
]
MISSING = [("1B06PS", "AR, EL, ST")]
AGE_DAYS = 2  # every copy in Parent\C06 is dated 17 Oct, the run was 19 Oct
FAR = 30

rows = [r for r in data if r and str(r[0]).isdigit()]
findings = []
for r in rows:
    no, sev, b, file_, check, what, value, action, ids = r
    findings.append(dict(sev=sev, b=b, file=file_ or "", check=check, what=what, value=value or "",
                         ids=[int(x) for x in str(ids or "").replace(" ", "").split(",") if x]))

cad, gen = {}, {}
for x in findings:
    if x["check"] == "CAD":
        cad.setdefault(x["file"], []).append(x)
    if x["check"] == "Generic Models":
        gen.setdefault(x["file"], []).append(x)

wb = Workbook()

# ================= Summary =================
ws = wb.active
ws.title = "Summary"
ws.sheet_properties.tabColor = NAVY
banner(ws, "C06   ME model checks", "Run 19 Oct 2026     Reference: the AR model of each building     18 ME models in 14 buildings", 8)

fix_models = [m for m in ME_MODELS if isinstance(m[2], float) and m[2] > FAR]
review_models = sorted(set(cad) | set(gen))
ws["B5"].value = f"{len(fix_models)} of {len(ME_MODELS)} ME models need a fix.  {len(MISSING)} building has no ME model.  {len(review_models)} models have CAD imports or Generic Models to review."
ws["B5"].font = Font(name=DEMI, size=12, color=NAVY)
ws.row_dimensions[5].height = 22

H = 7
header(ws, H, ["Check", "What is checked", "Checked", "With issue", "Status", "What the modeller does"],
       [18, 46, 12, 12, 12, 52])
cad_files = sum(len(v) for v in cad.values())
gen_elems = sum(sum(len(i["ids"]) for i in v) for v in gen.values())
score = [
    ("Coordinates", f"Within {FAR} m of the reference model of its building, on the shared site", 18, "models", len(fix_models), "FIX",
     "Export the NWC again from Revit on the project shared coordinates"),
    ("Missing model", "Every building that has other models has an ME model", 15, "buildings", len(MISSING), "FIX",
     "Issue the ME model for the building"),
    ("Naming", "Name is 1104-PAR-<building>-ZZZ-ME-MOD-<number>", 18, "models", 0, "OK", ""),
    ("File age", "NWC saved in the last 7 days", 18, "models", 0, "OK", ""),
    ("CAD imports", "No DWG, IPT or IAM imported into the model", 18, "models", len(cad), "REVIEW",
     "Replace each import with a Revit family, or delete it"),
    ("Generic Models", "No family left in the Generic Models category", 18, "models", len(gen), "REVIEW",
     "Move each family to its right category, for example Mechanical Equipment"),
]
for i, (name, what, n, unit, bad, st, act) in enumerate(score):
    r = H + 1 + i
    band = i % 2 == 1
    body(ws.cell(r, 2), name, band=band); ws.cell(r, 2).font = f(bold=True, color=NAVY)
    body(ws.cell(r, 3), what, band=band, wrap=True)
    one = unit[:-1]
    body(ws.cell(r, 4), n, fmt=f'[=1]0" {one}";0" {unit}"', align="right", band=band)
    body(ws.cell(r, 5), bad, fmt=f'[=1]0" {one}";0" {unit}";"none"', align="right", band=band)
    if bad:
        issue(ws.cell(r, 5), low=(st == "REVIEW"))
    chip(ws.cell(r, 6), st)
    body(ws.cell(r, 7), act or "Nothing to do", band=band, wrap=True,
         ink=INK if act else MUTED)
    ws.row_dimensions[r].height = 30

n = H + len(score) + 2
notes = [
    ("Models", "one row per ME model, red where a check failed"),
    ("To fix", "one row per thing to fix, High first, with the words to send the modeller"),
    ("Element IDs", "one element per row. Filter on Model, copy the IDs, then Revit Manage > Select by ID"),
]
ws.cell(n, 2, "Tabs").font = f(bold=True, color=NAVY)
for i, (tab, words) in enumerate(notes):
    ws.cell(n + 1 + i, 2, tab).font = f(bold=True, color=CYAN)
    ws.cell(n + 1 + i, 3, words).font = f(color=MUTED)
ws.cell(n + 5, 2, "Where a building has no AR model, the EL model is the reference (1B06WL). Where it has no AR or EL, the ME model is (1B06M1, 1B06PK, 1C06M2).").font = f(color=MUTED, italic=True)
print_setup(ws, 7, H)

# ================= Models =================
ws = wb.create_sheet("Models")
ws.sheet_properties.tabColor = CYAN
banner(ws, "C06   ME models", "One row per model.  Red: fix before the next clash run.  Amber: review.  Grey: nothing to do.", 12)
H = 5
cols = ["Building", "Model", "Status", "Compared with", "Off the reference (m)", "Naming", "File age (days)",
        "CAD imports (files)", "Generic Models (elements)", "File name"]
header(ws, H, cols, [11, 12, 11, 12, 13, 10, 11, 13, 15, 46])
ws.row_dimensions[H].height = 34
r = H
all_rows = [(bld(m[0]), m) for m in ME_MODELS] + [(b, None) for b, _ in MISSING]
all_rows.sort(key=lambda x: (x[0], x[1][0] if x[1] else ""))
for i, (b, m) in enumerate(all_rows):
    r += 1
    band = i % 2 == 1
    if m is None:
        body(ws.cell(r, 2), b, band=band)
        body(ws.cell(r, 3), "none", ink=MUTED, band=band)
        chip(ws.cell(r, 4), "MISSING")
        body(ws.cell(r, 5), "", band=band)
        for c in range(6, 11):
            body(ws.cell(r, c), "", band=band)
        body(ws.cell(r, 11), "No ME model. This building has AR, EL and ST.", ink=ISSUE_INK, band=band)
        continue
    name, ref, off = m
    n_cad = len(cad.get(name, []))
    n_gen = sum(len(x["ids"]) for x in gen.get(name, []))
    far = isinstance(off, float) and off > FAR
    st = "FIX" if far else ("REVIEW" if n_cad or n_gen else "OK")
    body(ws.cell(r, 2), b, band=band)
    body(ws.cell(r, 3), short(name), band=band); ws.cell(r, 3).font = f(bold=True, color=NAVY)
    chip(ws.cell(r, 4), st)
    body(ws.cell(r, 5), "itself" if off == "ref" else ref, align="center", band=band,
         ink=MUTED if off == "ref" else INK)
    if off == "ref":
        body(ws.cell(r, 6), "reference", align="right", ink=MUTED, band=band)
    elif off is None:
        body(ws.cell(r, 6), "under 1", align="right", ink=MUTED, band=band)
    else:
        body(ws.cell(r, 6), off, fmt="#,##0.0", align="right", band=band)
        if far:
            issue(ws.cell(r, 6))
    body(ws.cell(r, 7), "OK", align="center", ink=GREEN, band=band)
    body(ws.cell(r, 8), AGE_DAYS, fmt="0", align="center", band=band)
    body(ws.cell(r, 9), n_cad, fmt='0;;"-"', align="center", ink=INK if n_cad else MUTED, band=band)
    if n_cad:
        issue(ws.cell(r, 9), low=True)
    body(ws.cell(r, 10), n_gen, fmt='0;;"-"', align="center", ink=INK if n_gen else MUTED, band=band)
    if n_gen:
        issue(ws.cell(r, 10), low=True)
    body(ws.cell(r, 11), name, ink=MUTED, band=band)
ws.freeze_panes = ws.cell(H + 1, 4)
ws.auto_filter.ref = f"B{H}:K{r}"
print_setup(ws, 11, H)

# ================= To fix =================
ws = wb.create_sheet("To fix")
ws.sheet_properties.tabColor = RED
banner(ws, "C06   ME to fix", "One row per thing to fix.  High first.  Send the Fix words to the modeller as they are.", 11)
H = 5
cols = ["No.", "Priority", "Building", "Model", "Check", "Finding", "Value", "Unit", "Fix"]
header(ws, H, cols, [6, 10, 11, 12, 15, 52, 10, 10, 50])
items = []
for b, others in MISSING:
    items.append(("High", b, "none", "Missing model", f"No ME model. AR, EL and ST are issued", None, "",
                  "Issue the ME model for this building"))
for name, ref, off in ME_MODELS:
    if isinstance(off, float) and off > FAR:
        x = next(v for v in findings if v["file"] == name and v["check"] == "Coordinates")
        sites = re.findall(r"on (.+?)(?:,|$)", x["value"])
        items.append(("High", bld(name), short(name), "Coordinates",
                      f"{off:,.1f} m from the {ref} model. ME on site {sites[0]}, {ref} on {sites[1]}",
                      off, "m", "Export the NWC again on the project shared coordinates"))
for name, xs in cad.items():
    for x in xs:
        items.append(("Low", bld(name), short(name), "CAD import", x["what"].replace("CAD file ", "Imported ") ,
                      len(x["ids"]), "elements", "Replace with a Revit family, or delete it"))
for name, xs in gen.items():
    for x in xs:
        fam = re.sub(r"^Generic Models family ", "", x["what"])
        items.append(("Low", bld(name), short(name), "Generic Models", f"Family {fam.replace(', type ', ', type ')}",
                      len(x["ids"]), "elements", "Move the family to its right category"))
order = {"High": 0, "Medium": 1, "Low": 2}
items.sort(key=lambda t: (order[t[0]], t[1], t[2], t[3]))
r = H
for i, (sev, b, mdl, chk, finding, val, unit, fix) in enumerate(items):
    r += 1
    band = i % 2 == 1
    body(ws.cell(r, 2), i + 1, align="right", ink=MUTED, band=band)
    chip(ws.cell(r, 3), sev, PRIORITY)
    body(ws.cell(r, 4), b, band=band)
    body(ws.cell(r, 5), mdl, band=band); ws.cell(r, 5).font = f(bold=True, color=NAVY)
    body(ws.cell(r, 6), chk, band=band)
    body(ws.cell(r, 7), finding, band=band, wrap=True)
    body(ws.cell(r, 8), val, fmt="#,##0.0" if unit == "m" else "0", align="right", band=band)
    body(ws.cell(r, 9), unit[:-1] if val == 1 else unit, ink=MUTED, band=band)
    body(ws.cell(r, 10), fix, band=band, wrap=True)
ws.freeze_panes = ws.cell(H + 1, 1)
ws.auto_filter.ref = f"B{H}:J{r}"
print_setup(ws, 10, H)

# ================= Element IDs =================
ws = wb.create_sheet("Element IDs")
ws.sheet_properties.tabColor = GREY
banner(ws, "C06   ME element IDs", "One element per row.  Filter on Model, copy the Element ID cells, then in Revit: Manage > Select by ID.", 8)
H = 5
cols = ["Building", "Model", "Check", "CAD file or family", "Type", "Element ID"]
header(ws, H, cols, [11, 12, 16, 44, 32, 13])
r = H
k = 0
for name in sorted(set(cad) | set(gen)):
    for x in cad.get(name, []) + gen.get(name, []):
        if x["check"] == "CAD":
            src, typ, chk = x["what"].replace("CAD file ", ""), "", "CAD import"
        else:
            m = re.match(r"Generic Models family (.+?), type (.+)$", x["what"])
            src, typ, chk = m.group(1), m.group(2), "Generic Models"
        for eid in x["ids"]:
            r += 1
            band = k % 2 == 1
            k += 1
            body(ws.cell(r, 2), bld(name), band=band)
            body(ws.cell(r, 3), short(name), band=band); ws.cell(r, 3).font = f(bold=True, color=NAVY)
            body(ws.cell(r, 4), chk, band=band)
            body(ws.cell(r, 5), src, band=band)
            body(ws.cell(r, 6), typ, ink=MUTED, band=band)
            body(ws.cell(r, 7), eid, fmt="0", align="right", band=band)
ws.freeze_panes = ws.cell(H + 1, 1)
ws.auto_filter.ref = f"B{H}:G{r}"
print_setup(ws, 7, H)

for s in wb.worksheets:
    s.sheet_view.zoomScale = 100
wb.active = 0
wb.properties.title = "C06 ME model checks"
wb.properties.creator = "NWC Federator"
wb.save(OUT)
print("saved", OUT, "items", len(items), "element rows", k)
