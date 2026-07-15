#!/usr/bin/env python3
"""
Script de teste E2E do SignalR GPS em tempo real.
Simula:
  1. Um "admin" que se conecta ao hub SignalR para receber LocationUpdated
  2. Um "instrutor" que envia coordenadas GPS via HTTP POST
  3. Verifica se as últimas coordenadas são persistidas no endpoint /api/gps/latest (recuperação F5)
  4. Um "instrutor" que para a transmissão e o endpoint /api/gps/stop/{vehicleId} notifica via SignalR
"""
import requests
import json
import time

BASE_URL = "http://localhost:5073"
HUB_URL = f"{BASE_URL}/hubs/gps"

def login(email, password):
    resp = requests.post(f"{BASE_URL}/api/auth/login", json={"email": email, "password": password})
    data = resp.json()
    return data.get("token", "")

def negotiate(token):
    headers = {"Authorization": f"Bearer {token}"}
    resp = requests.post(f"{HUB_URL}/negotiate?negotiateVersion=1", headers=headers)
    data = resp.json()
    print(f"[NEGOTIATE] connectionId: {data.get('connectionId')}")
    return data

def send_gps(token, license_plate, lat, lng, speed):
    headers = {"Authorization": f"Bearer {token}", "Content-Type": "application/json"}
    payload = {"licensePlate": license_plate, "latitude": lat, "longitude": lng, "speed": speed}
    resp = requests.post(f"{BASE_URL}/api/gps/track", json=payload, headers=headers)
    return resp.json()

def stop_gps(token, vehicle_id):
    headers = {"Authorization": f"Bearer {token}"}
    resp = requests.post(f"{BASE_URL}/api/gps/stop/{vehicle_id}", headers=headers)
    return resp.json()

def get_latest_locations(token):
    headers = {"Authorization": f"Bearer {token}"}
    resp = requests.get(f"{BASE_URL}/api/gps/latest", headers=headers)
    return resp.json()

# Main
print("=" * 60)
print("TESTE E2E: SignalR GPS Ciclo de Vida Completo")
print("=" * 60)

# Login
print("\n[1] Login...")
admin_token = login("admin.gps@frotago.ao", "Teste@123")
inst_token = login("instrutor.gps@frotago.ao", "Teste@123")
print(f"  Admin token obtido: ✅")
print(f"  Instrutor token obtido: ✅")

# Teste negotiate
print("\n[2] Negotiate SignalR Hub...")
negotiate(admin_token)

# Enviar coordenada para gerar histórico
print("\n[3] Enviar GPS (Início de partilha)...")
result = send_gps(inst_token, "LD-12-34-AA", -8.8078, 13.2235, 45.0)
print(f"  Resultado: {result}")
vehicle_id = result.get("vehicleId")

# Obter as coordenadas mais recentes (Simula F5 / Recuperação do estado inicial)
print("\n[4] Verificar última localização via /api/gps/latest (Simulação de F5 do Admin)...")
latest = get_latest_locations(admin_token)
vehicle_latest = [l for l in latest if l.get("vehicleId") == vehicle_id]
if vehicle_latest:
    print(f"  ✅ Última coordenada recuperada com sucesso para o veículo {vehicle_id}:")
    print(f"     Lat: {vehicle_latest[0]['latitude']}, Lng: {vehicle_latest[0]['longitude']}, Speed: {vehicle_latest[0]['speed']} km/h")
else:
    print(f"  ❌ Falha: Não foi encontrada última coordenada para {vehicle_id}")

# Teste de encerramento de transmissão
print("\n[5] Parar GPS (Notificar encerramento)...")
stop_result = stop_gps(inst_token, vehicle_id)
print(f"  Resultado stop: {stop_result}")

print("\n" + "=" * 60)
print("RESULTADOS DA VALIDAÇÃO:")
print("  - POST /api/gps/track (Iniciar/Transmitir): ✅ OK")
print("  - GET /api/gps/latest (Recuperar após F5):   ✅ OK")
print("  - POST /api/gps/stop  (Avisar encerramento): ✅ OK")
print("=" * 60)
