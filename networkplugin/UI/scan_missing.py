import os, re

root = r'C:\Users\oft-Enginner PC\Desktop\LBol_demoplugin\networkplugin\UI'

cs_files = []
for dirpath, dirnames, filenames in os.walk(root):
    for fn in filenames:
        if fn.endswith('.cs') and fn != 'scan_missing.py':
            cs_files.append(os.path.join(dirpath, fn))

cs_files.sort()

DECL = re.compile(
    r'^\s*(public|internal|protected)\s'
    r'(static\s|abstract\s|virtual\s|override\s|readonly\s|sealed\s|new\s|unsafe\s|partial\s|async\s)*'
    r'((class|interface|struct|enum|delegate)\s|'
    r'((void|[A-Za-z_][A-Za-z0-9_<>\[\],\s]*)\s+[A-Za-z_][A-Za-z0-9_<>]+\s*[\(<\{]))',
    re.MULTILINE
)

def has_xml(lines, idx):
    j = idx - 1
    while j >= 0:
        line = lines[j].strip()
        if line == '':
            j -= 1
            continue
        if line.startswith('[') and line.endswith(']'):
            j -= 1
            continue
        if '///' in line:
            return True
        return False
    return False

total_missing = 0
for fp in cs_files:
    with open(fp, 'r', encoding='utf-8', errors='replace') as f:
        content = f.read()
    lines = content.split('\n')
    missing = []
    i = 0
    while i < len(lines):
        line = lines[i]
        m = DECL.match(line)
        if m:
            if not has_xml(lines, i):
                decl_name = m.group(0).strip()
                if not any(kw in line for kw in ['get;', 'set;', 'get =>', 'set =>']):
                    missing.append((i + 1, decl_name))
        i += 1
    if missing:
        rel = os.path.relpath(fp, root)
        total_missing += len(missing)
        for lineno, decl in missing:
            print(f'{rel}:L{lineno}: {decl[:120]}')

print(f'\n总计 {total_missing} 处缺少 XML 注释')
