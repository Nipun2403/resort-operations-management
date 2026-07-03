#!/usr/bin/env python3

"""Verification script for Architecture_GptOss.md.

The script extracts all markdown code blocks that are preceded by a reference line
in the format:
    [FilePath](./FilePath) : lines A-B
It then reads the corresponding source file, extracts the same line range, and
compares the content with the code block. Any mismatches are reported.

The script is idempotent and can be run repeatedly. It will exit with status 0
if no mismatches are found, otherwise with status 1.
"""

import re
import sys
from pathlib import Path

DOC_PATH = Path("Documents/Architecture_GptOss.md")

if not DOC_PATH.is_file():
    print(f"Document not found: {DOC_PATH}", file=sys.stderr)
    sys.exit(1)

content = DOC_PATH.read_text(encoding="utf-8")

# Regex to capture reference lines and the following fenced code block
pattern = re.compile(
    r"\[([^\]]+)\]\(([^)]+)\)\s*:\s*lines\s*(\d+)-(\d+)"  # reference line
    r"\s*\n```[a-z]*\n(.*?)\n```",  # code block (non-greedy)
    re.DOTALL,
)

mismatches = []

for match in pattern.finditer(content):
    display_path, relative_path, start_line, end_line, code_block = match.groups()
    start_line = int(start_line)
    end_line = int(end_line)
    # Resolve the file path relative to the repository root
    file_path = Path(relative_path)
    if not file_path.is_file():
        mismatches.append((relative_path, "File not found"))
        continue
    # Read the source lines (1-indexed inclusive)
    source_lines = file_path.read_text(encoding="utf-8").splitlines()
    # Adjust for 1-indexed lines
    extracted = "\n".join(source_lines[start_line - 1 : end_line])
    # Normalize line endings for comparison
    extracted = extracted.strip().replace("\r\n", "\n")
    code_block = code_block.strip().replace("\r\n", "\n")
    if extracted != code_block:
        mismatches.append((relative_path, f"Lines {start_line}-{end_line} do not match"))

if mismatches:
    print("Verification mismatches found:")
    for path, reason in mismatches:
        print(f"- {path}: {reason}")
    sys.exit(1)
else:
    print("All references verified successfully.")
    sys.exit(0)
