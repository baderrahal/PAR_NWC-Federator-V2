# Q133, 2026-10-05. The mirrored pairs F132's rule finds in a clash XML, by Bader's answers to
# Q114 and Q121: two tests whose sides are the same two sets swapped, and two tests whose sides
# carry the same two rule lists, a set's rule list being its findspec as the XML writes it. Also
# a test whose two sides are one set, or carry one rule list. Read only. Run as
#
#   python q133-rule-pairs.py <out.tsv> <xml> [<xml> ...]
#
# It writes one line per pair or self test per XML, tab separated: the XML's file name, the kind,
# the first test, the second test, the first's two set names and the second's, and a summary per
# XML on lines starting with #.

import hashlib
import sys
import xml.etree.ElementTree as ET


def canon(element):
    # The findspec with every text and tail stripped, so line breaks and indents do not count.
    copy = ET.fromstring(ET.tostring(element))
    for e in copy.iter():
        e.text = (e.text or "").strip() or None
        e.tail = None
    return ET.tostring(copy, encoding="unicode")


def sets_of(root):
    found = {}

    def walk(node, path):
        for child in node:
            if child.tag == "viewfolder":
                walk(child, path + [child.get("name")])
            elif child.tag == "selectionset":
                locator = "lcop_selection_set_tree/" + "/".join(path + [child.get("name")])
                spec = child.find("findspec")
                found[locator] = (child.get("name"), canon(spec) if spec is not None else "NO FINDSPEC " + locator)

    for holder in root.iter("selectionsets"):
        walk(holder, [])
    return found


def side(test, which):
    locators = [l.text for l in test.findall(which + "/clashselection/locator")]
    return locators[0] if len(locators) == 1 else "MORE THAN ONE " + "|".join(locators)


def main():
    out = open(sys.argv[1], "w", encoding="utf-8", newline="\n")
    for path in sys.argv[2:]:
        data = open(path, "rb").read()
        name = path.replace("\\", "/").split("/")[-1] + " sha256 " + hashlib.sha256(data).hexdigest()[:8]
        root = ET.fromstring(data)
        sets = sets_of(root)
        tests = []
        for t in root.iter("clashtest"):
            tests.append((t.get("name"), side(t, "left"), side(t, "right")))
        rule = lambda loc: sets[loc][1] if loc in sets else "UNRESOLVED " + loc
        setname = lambda loc: sets[loc][0] if loc in sets else "UNRESOLVED " + loc
        lines = []
        swapped = 0
        same_rules = 0
        self_set = 0
        self_rules = 0
        by_locs = {}
        by_rules = {}
        for i, (n, a, b) in enumerate(tests):
            if a == b:
                self_set += 1
                lines.append((name, "one set on both sides", n, "", setname(a) + " | " + setname(b), ""))
            elif rule(a) == rule(b):
                self_rules += 1
                lines.append((name, "one rule list on both sides", n, "", setname(a) + " | " + setname(b), ""))
            by_locs.setdefault(tuple(sorted((a, b))), []).append(i)
            by_rules.setdefault(tuple(sorted((rule(a), rule(b)))), []).append(i)
        for key, members in by_rules.items():
            for x in range(len(members)):
                for y in range(x + 1, len(members)):
                    n1, a1, b1 = tests[members[x]]
                    n2, a2, b2 = tests[members[y]]
                    if (a1, b1) == (b2, a2):
                        kind = "swapped sets"
                        swapped += 1
                    elif (a1, b1) == (a2, b2):
                        kind = "the same two sets in the same order"
                        swapped += 1
                    else:
                        kind = "the same rule lists, other sets"
                        same_rules += 1
                    lines.append((name, kind, n1, n2, setname(a1) + " | " + setname(b1), setname(a2) + " | " + setname(b2)))
        groups = {}
        for loc, (sname, spec) in sets.items():
            groups.setdefault(spec, []).append(sname)
        shared = [sorted(v) for v in groups.values() if len(v) > 1]
        out.write("# " + name + "  sha256 " + hashlib.sha256(data).hexdigest() + "  tests " + str(len(tests)) + "  sets " + str(len(sets)) + "\n")
        out.write("# " + name + "  distinct unordered set pairs " + str(len(by_locs)) + ", distinct unordered rule list pairs " + str(len(by_rules)) + "\n")
        out.write("# " + name + "  sets sharing one rule list: " + ("none" if not shared else " / ".join(" and ".join(s) for s in shared)) + "\n")
        out.write("# " + name + "  pairs by swapped or repeated sets " + str(swapped) + ", pairs by the same rule lists over other sets " + str(same_rules)
                  + ", tests with one set on both sides " + str(self_set) + ", tests with one rule list on both sides " + str(self_rules) + "\n")
        for l in lines:
            out.write("\t".join(l) + "\n")
    out.close()


main()
