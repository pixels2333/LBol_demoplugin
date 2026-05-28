import re, os

root = r'C:\Users\oft-Enginner PC\Desktop\LBol_demoplugin\networkplugin\UI'
pattern = rb'\{\"\$mid[^}]*\}'
count = 0

for dirpath, dirnames, filenames in os.walk(root):
    for fn in filenames:
        if fn.endswith('.cs') and fn != 'clean_json.py' and fn != 'scan_missing.py':
            fp = os.path.join(dirpath, fn)
            with open(fp, 'rb') as f:
                data = f.read()
            new_data = re.sub(pattern, b'', data)
            new_data = new_data.rstrip() + b'\n'
            if new_data != data:
                with open(fp, 'wb') as f:
                    f.write(new_data)
                count += 1

print(f'Cleaned {count} files')
