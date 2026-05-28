import re
fp = r'C:\Users\oft-Enginner PC\Desktop\LBol_demoplugin\networkplugin\UI\Widgets\TradeSlotWidget.cs'
with open(fp, 'rb') as f:
    d = f.read()
# Remove JSON artifacts
d = re.sub(rb'\{[^}]*?\$mid[^}]*?\}', b'', d)
d = d.rstrip() + b'\n'
# Fix: add missing property name ("Card") for expression-bodied property
# Change: "public Card => _currentCard;" to "public Card => _currentCard;"
old = b'public Card => _currentCard;'
new = b'public Card => _currentCard;'
if old in d:
    d = d.replace(old, new, 1)
    print('Fixed Card property')
else:
    print('Pattern not found, checking context...')
    for m in re.finditer(rb'public.*?Card.*?=>', d):
        print(f'  Found: {m.group()}')
    idx = d.find(b'Card =>')
    if idx >= 0:
        print(f'Context: {d[idx-20:idx+30]}')
with open(fp, 'wb') as f:
    f.write(d)
print('Done')
