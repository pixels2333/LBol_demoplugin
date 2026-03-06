#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""NetworkPlugin 代码行数统计脚本"""

from pathlib import Path
from collections import defaultdict
import os

def count_lines_in_file(file_path):
    """统计文件行数"""
    try:
        with open(file_path, 'r', encoding='utf-8', errors='ignore') as f:
            return len(f.readlines())
    except:
        return 0

def main():
    root = Path('.')
    
    # 按文件类型统计
    stats = defaultdict(lambda: {'files': 0, 'lines': 0})
    
    # 按目录统计 (仅 .cs 文件)
    dir_stats = defaultdict(lambda: {'files': 0, 'lines': 0})
    
    total_lines = 0
    
    # 遍历所有文件
    for file_path in root.rglob('*'):
        if not file_path.is_file():
            continue
        
        ext = file_path.suffix
        if ext not in ['.cs', '.xaml', '.xml', '.json', '.md']:
            continue
        
        lines = count_lines_in_file(file_path)
        
        # 按类型统计
        stats[ext]['files'] += 1
        stats[ext]['lines'] += lines
        total_lines += lines
        
        # 按目录统计
        if ext == '.cs':
            rel_path = file_path.relative_to(root)
            dir_name = str(rel_path.parts[0]) if rel_path.parts else 'root'
            dir_stats[dir_name]['files'] += 1
            dir_stats[dir_name]['lines'] += lines
    
    # 打印结果
    print("=" * 70)
    print("NetworkPlugin 代码行数统计报告")
    print("=" * 70)
    print()
    
    print("【按文件类型统计】")
    print()
    for ext in sorted(stats.keys()):
        count = stats[ext]['files']
        lines = stats[ext]['lines']
        print(f"  {ext:10s}: {count:3d} 个文件  {lines:7d} 行")
    
    print()
    print(f"  总计: {total_lines:,} 行")
    print()
    
    # 统计 C# 文件目录分布
    print("【C# 文件按目录统计】")
    print()
    
    cs_total = 0
    for dir_name in sorted(dir_stats.keys()):
        count = dir_stats[dir_name]['files']
        lines = dir_stats[dir_name]['lines']
        cs_total += lines
        print(f"  {dir_name:20s}: {count:3d} 个文件  {lines:7d} 行")
    
    print()
    print(f"  C# 文件总计: {cs_total:,} 行")
    print("=" * 70)

if __name__ == '__main__':
    main()
