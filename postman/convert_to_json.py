#!/usr/bin/env python3
"""
Script para converter requests YAML do Postman para JSON importável.
Gera URLs no formato simples: {{baseUrl}}/endpoint
"""

import os
import json
from pathlib import Path
from typing import Dict, List, Any

import yaml

class PostmanConverter:
    def __init__(self, collections_path: str):
        self.collections_path = collections_path
        self.collection_items = []
        self.folders = {}
        
    def parse_yaml_file(self, file_path: str) -> Dict[str, Any]:
        """Parse YAML file and convert to dict."""
        try:
            with open(file_path, 'r', encoding='utf-8') as f:
                return yaml.safe_load(f)
        except Exception as e:
            print(f"Error parsing {file_path}: {e}")
            return {}
    
    def get_folder_structure(self, file_path: str) -> List[str]:
        """Extract folder hierarchy from file path."""
        rel_path = os.path.relpath(file_path, self.collections_path)
        parts = Path(rel_path).parts[:-1]  # Exclude filename
        return [p for p in parts if p and p != "PHC API"]
    
    def create_request_object(self, request_data: Dict[str, Any], request_name: str) -> Dict[str, Any]:
        """Convert YAML request to Postman request object."""
        
        # Extract method (default to GET)
        method = request_data.get('method', 'GET').upper()
        
        # Extract URL
        url_value = request_data.get('url', '')
        
        # Create header list
        headers = []
        if 'headers' in request_data and request_data['headers']:
            for header in request_data['headers']:
                if isinstance(header, dict):
                    headers.append({
                        "key": header.get('key', ''),
                        "value": header.get('value', ''),
                        "disabled": header.get('disabled', False)
                    })
        
        # Create query params list
        query_params = []
        if 'queryParams' in request_data and request_data['queryParams']:
            for param in request_data['queryParams']:
                if isinstance(param, dict):
                    query_params.append({
                        "key": param.get('key', ''),
                        "value": param.get('value', '')
                    })
        
        # Handle body
        body_obj = None
        body_data = request_data.get('body', {})
        if body_data:
            body_type = body_data.get('type', 'json')
            content = body_data.get('content', '')
            
            if body_type == 'json' and content:
                body_obj = {
                    "mode": "raw",
                    "raw": content,
                    "options": {"raw": {"language": "json"}}
                }
            elif body_type == 'xml' and content:
                body_obj = {
                    "mode": "raw",
                    "raw": content,
                    "options": {"raw": {"language": "xml"}}
                }
            elif body_type == 'form' and content:
                body_obj = {
                    "mode": "formdata",
                    "formdata": content if isinstance(content, list) else []
                }
        
        # Handle authentication
        auth_obj = None
        auth_data = request_data.get('auth', {})
        if auth_data:
            auth_type = auth_data.get('type', 'noauth')
            
            if auth_type == 'bearer':
                credentials = auth_data.get('credentials', {})
                token = credentials.get('token', '') if isinstance(credentials, dict) else ''
                auth_obj = {
                    "type": "bearer",
                    "bearer": [{"key": "token", "value": token, "type": "string"}]
                }
            elif auth_type == 'noauth':
                auth_obj = {"type": "noauth"}
        
        # Build URL (sempre simples: {{baseUrl}}/...)
        url_string = url_value
        if query_params:
            # Só acrescenta query se ainda não existir na URL
            # (evita duplicar pageSize/page etc.)
            if '?' not in url_string:
                param_strings = [f"{p['key']}={p['value']}" for p in query_params]
                url_string = f"{url_value}?{'&'.join(param_strings)}"
        
        # Create request object (url como string para máxima compatibilidade)
        request_obj = {
            "name": request_name,
            "request": {
                "method": method,
                "header": headers,
                "url": url_string
            }
        }
        
        if body_obj:
            request_obj["request"]["body"] = body_obj
        
        if auth_obj:
            request_obj["request"]["auth"] = auth_obj
        
        return request_obj
    
    def get_or_create_folder(self, folder_path: List[str]) -> Dict[str, Any]:
        """Get or create a folder object."""
        folder_key = " > ".join(folder_path)
        
        if folder_key not in self.folders:
            self.folders[folder_key] = {
                "name": folder_path[-1] if folder_path else "Root",
                "item": []
            }
        
        return self.folders[folder_key]
    
    def convert_all(self) -> Dict[str, Any]:
        """Convert all YAML files to Postman collection."""
        
        # Find all request files
        request_files = []
        for root, dirs, files in os.walk(self.collections_path):
            for file in files:
                if file.endswith('.request.yaml'):
                    request_files.append(os.path.join(root, file))
        
        print(f"Found {len(request_files)} request files")
        
        # Process each file
        for file_path in sorted(request_files):
            yaml_data = self.parse_yaml_file(file_path)
            if not yaml_data:
                continue
            
            # Get request name
            request_name = yaml_data.get('name', Path(file_path).stem)
            
            # Get folder structure
            folders = self.get_folder_structure(file_path)
            
            # Create request object
            request_obj = self.create_request_object(yaml_data, request_name)
            
            # Add to appropriate folder
            if folders:
                folder = self.get_or_create_folder(folders)
                folder["item"].append(request_obj)
            else:
                self.collection_items.append(request_obj)
            
            print(f"✓ Converted: {request_name} ({' > '.join(folders) if folders else 'Root'})")
        
        # Build folder hierarchy
        root_items = list(self.collection_items)
        
        # Organize folders
        first_level_folders = {}
        for folder_key, folder_data in self.folders.items():
            parts = folder_key.split(" > ")
            if len(parts) == 1:
                first_level_folders[parts[0]] = folder_data
            else:
                first_level_folders.setdefault(parts[0], {
                    "name": parts[0],
                    "item": [],
                    "_postman_isSubFolder": True
                })
                if len(parts) == 2:
                    first_level_folders[parts[0]]["item"].append(folder_data)
        
        root_items.extend(first_level_folders.values())
        
        # Create collection (com variáveis do projeto)
        collection = {
            "info": {
                "name": "PHC API",
                "description": "PHC API Collection",
                "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
            },
            "item": root_items,
            "variable": [
                {"key": "baseUrl", "value": "https://tecnica.mz.2business-apps.com:444/tecnica/2B/PHCAPIWS/api", "type": "string"},
                {"key": "accessToken", "value": "", "type": "string"},
                {"key": "adminAcessToken", "value": "", "type": "string"},
                {"key": "username", "value": "", "type": "string"},
                {"key": "password", "value": "", "type": "string"},
                {"key": "referencia", "value": "P12", "type": "string"}
            ]
        }
        
        return collection


def main():
    postman_path = r"c:\Users\imunguambe.2BUSINESS\Documents\GitHub\PHCAPI\postman"
    collections_path = os.path.join(postman_path, "collections", "PHC API")
    
    print("Starting Postman collection conversion...")
    print(f"Collections path: {collections_path}")
    
    converter = PostmanConverter(collections_path)
    collection = converter.convert_all()
    
    # Save to JSON
    output_path = os.path.join(postman_path, "PHC_API_Collection.json")
    with open(output_path, 'w', encoding='utf-8') as f:
        json.dump(collection, f, indent=2, ensure_ascii=False)
    
    print(f"\n✓ Collection saved to: {output_path}")
    print(f"Total items: {len(converter.collection_items)} root items + {len(converter.folders)} folders")


if __name__ == "__main__":
    main()
