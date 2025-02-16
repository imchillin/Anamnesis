import json
import os
from pathlib import Path

def log(message):
    print(f"[LOG] {message}")

def extract_author_from_url(url):
    # Extract filename from the URL
    filename = os.path.basename(url)
    
    # Identify the author as the part before the first dash
    parts = filename.split('-')
    if len(parts) < 2:
        return "Unknown"
    
    author = parts[0].replace('_', ' ')
    return author

def convert_txt_to_json(txt_file, json_file):
    entries = []
    existing_entries = set()
    
    # Load existing JSON data if file exists
    if os.path.exists(json_file):
        with open(json_file, 'r', encoding='utf-8') as file:
            try:
                existing_entries_list = json.load(file)
                existing_entries = {entry['Url'] for entry in existing_entries_list}
                entries.extend(existing_entries_list)
                log(f"Loaded {len(existing_entries_list)} existing entries from {json_file}")
            except json.JSONDecodeError:
                log("Existing JSON file is empty or malformed. Starting fresh.")
                pass  # If the file is empty or malformed, proceed with an empty list
    
    new_entries_count = 0
    with open(txt_file, 'r', encoding='utf-8') as file:
        for line in file:
            url = line.strip()
            if not url:
                continue
            if url in existing_entries:
                log(f"Skipping duplicate URL: {url}")
                continue
            
            author = extract_author_from_url(url)
            file_name, file_ext = os.path.splitext(url)
            thumbnail_url = f"{file_name}.md{file_ext}"
            
            entry = {
                "Url": url,
                "Author": author,
                "Thumbnail": thumbnail_url
            }
            entries.append(entry)
            existing_entries.add(url)
            new_entries_count += 1
    
    with open(json_file, 'w', encoding='utf-8') as file:
        json.dump(entries, file, indent=4)
    
    log(f"Added {new_entries_count} new entries. Total entries: {len(entries)}")

# Grab Absolute Path and start script
parent_path = Path(__file__).resolve().parent.parent
log("Starting conversion process...")
convert_txt_to_json(parent_path / 'galleryLinks.txt', parent_path / 'Anamnesis/Data/Images.json')
log("Conversion process completed.")
