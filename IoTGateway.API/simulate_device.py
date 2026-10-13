import ssl
import time
import json
import random
import paho.mqtt.client as mqtt

# Configurazione Certificati e Broker
BROKER_HOST = "localhost"
BROKER_PORT = 8883
SERIAL_NUMBER = "DEV-ESP32-001"
TOPIC = f"devices/{SERIAL_NUMBER}/telemetry"

CA_CERT = "./certs/ca.crt"
CLIENT_CERT = "./certs/device01.crt"
CLIENT_KEY = "./certs/device01.key"

def on_connect(client, userdata, flags, rc, properties=None):
    if rc == 0:
        print(f"[+] Connesso con successo al Broker MQTT con mTLS (Porta {BROKER_PORT})!")
    else:
        print(f"[-] Errore di connessione. Codice: {rc}")

client = mqtt.Client(mqtt.CallbackAPIVersion.VERSION2, client_id=SERIAL_NUMBER)
client.on_connect = on_connect

# Configurazione mTLS (Richiede Certificato CA + Certificato e Chiave Client)
client.tls_set(
    ca_certs=CA_CERT,
    certfile=CLIENT_CERT,
    keyfile=CLIENT_KEY,
    cert_reqs=ssl.CERT_REQUIRED,
    tls_version=ssl.PROTOCOL_TLSv1_2
)
client.tls_insecure_set(True) # Disabilita verifica del nome host per localhost

print("[*] Connessione al broker in corso...")
client.connect(BROKER_HOST, BROKER_PORT, 60)
client.loop_start()

try:
    while True:
        temperature = round(random.uniform(18.0, 28.5), 2)
        payload = {
            "metric": "temperature",
            "value": temperature
        }
        
        print(f"[>] Invio telemetria mTLS: {payload}")
        client.publish(TOPIC, json.dumps(payload))
        time.sleep(3)

except KeyboardInterrupt:
    print("\n[*] Arresto simulatore...")
    client.loop_stop()
    client.disconnect()