fp = r'C:\Users\oft-Enginner PC\Desktop\LBol_demoplugin\networkplugin\UI\Widgets\TradeSlotWidget.cs'
with open(fp, 'r', encoding='utf-8') as f:
    data = f.read()
# Line 68 is broken: 'public Card => _currentCard;' — missing property name
# Fix: add the property name 'Card' after the type 'Card'
import re
data = re.sub(r'^(\s*public) Card (=> _currentCard;)', r'\1 Card \2', data, flags=re.MULTILINE)
with open(fp, 'w', encoding='utf-8') as f:
    f.write(data)
print('Fixed!')
