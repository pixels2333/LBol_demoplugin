import sys

filepath = sys.argv[1]
with open(filepath, 'r', encoding='utf-8') as f:
    lines = f.readlines()

# Remove trailing garbage lines that contain cache artifacts
clean = []
for line in lines:
    if 'cache_control' in line or '$mid' in line:
        break
    clean.append(line)

# Find the last #endregion and ensure proper class close
found_endregion = False
result = []
for line in clean:
    if '#endregion' in line:
        found_endregion = True
    result.append(line)

# If we found endregion, ensure the file ends with only }
# Remove any trailing }n or }n\n patterns
if found_endregion and result:
    # Remove anything after the last #endregion line
    last_idx = None
    for i, line in enumerate(result):
        if '#endregion' in line:
            last_idx = i
    
    if last_idx is not None:
        result = result[:last_idx + 1]
        # Make sure the last line doesn't have }n
        last = result[-1]
        # Remove any characters after the actual content
        result[-1] = last.rstrip() + '\n'
        # Add closing brace
        result.append('}\n')

with open(filepath, 'w', encoding='utf-8') as f:
    f.writelines(result)

print(f'Done: {len(result)} lines written')
