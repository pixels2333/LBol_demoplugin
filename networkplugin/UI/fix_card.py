fp = r'C:\Users\oft-Enginner PC\Desktop\LBol_demoplugin\networkplugin\UI\Widgets\TradeSlotWidget.cs'
with open(fp, 'r', encoding='utf-8') as f:
    data = f.read()
# The original was 'public Card => _currentCard;'
# Currently broken as 'public Card => _currentCard;'
old_broken = 'public Card => _currentCard;'
new_fixed = 'public Card => _currentCard;'
if old_broken in data:
    data = data.replace(old_broken, new_fixed)
    with open(fp, 'w', encoding='utf-8') as f:
        f.write(data)
    print('Fixed!')
elif 'public Card' in data:
    print('Already correct')
else:
    print('Cannot find pattern')
    # Show context
    idx = data.find('Card =>')
    if idx >= 0:
        print('Context:', repr(data[idx-20:idx+30]))
