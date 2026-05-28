import re, os, sys

root = r'C:\Users\oft-Enginner PC\Desktop\LBol_demoplugin\networkplugin\UI'
pattern = re.compile(rb'\{[^}]*\$mid[^}]*\}')
count = 0

for dirpath, dirnames, filenames in os.walk(root):
    for fn in filenames:
        if not fn.endswith('.cs') or fn in ('fix_all.py', 'clean_json.py', 'scan_missing.py'):
            continue
        fp = os.path.join(dirpath, fn)
        with open(fp, 'rb') as f:
            data = f.read()
        
        new_data = pattern.sub(b'', data)
        new_data = new_data.rstrip() + b'\n'
        
        if new_data != data:
            with open(fp, 'wb') as f:
                f.write(new_data)
            count += 1
            print(f'Fixed: {os.path.relpath(fp, root)}')

print(f'Total: {count} files fixed')
