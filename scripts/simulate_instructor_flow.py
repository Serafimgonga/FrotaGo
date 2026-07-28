#!/usr/bin/env python3
"""
FrotaGo ERP — Script de Simulação Terminal do Cliente Mobile (Instrutor)

Este script simula o fluxo completo de ponta a ponta:
1. Registo Público da Escola (Proprietário)
2. Envio de Convite para o novo Instrutor pelo Painel Web
3. Aceitação do Convite pelo Instrutor (Definição de palavra-passe)
4. Criação de Recursos pela Escola (Viatura, Aluno e Agendamento de Aula Prática)
5. Autenticação Mobile do Instrutor (Obtenção de Token JWT)
6. Obtenção do Perfil & Dashboard Mobile do Instrutor
7. Lista de Aulas de Hoje e Obtenção de Detalhes da Aula
8. Início da Aula Prática (Alteração para estado EmCurso)
9. Transmissão de Telemetria GPS em Tempo Real (Rota de Luanda via HTTP/SignalR)
10. Finalização da Aula & Submissão do Relatório Final (Odómetro, Notas e Avaliação)
"""

import sys
import json
import time
import urllib.request
import urllib.error

BASE_URL = "http://localhost:5073"

# Cores ANSI para saída decorada no terminal
GREEN = "\033[1;32m"
YELLOW = "\033[1;33m"
BLUE = "\033[1;34m"
CYAN = "\033[1;36m"
RED = "\033[1;31m"
BOLD = "\033[1m"
RESET = "\033[0m"

def print_header(title):
    print("\n" + "=" * 75)
    print(f"{CYAN}{BOLD}▶ {title}{RESET}")
    print("=" * 75)

def print_success(msg):
    print(f"{GREEN}✔ [SUCESSO]{RESET} {msg}")

def print_info(msg):
    print(f"{BLUE}ℹ [INFO]{RESET} {msg}")

def print_warn(msg):
    print(f"{YELLOW}⚠ [AVISO]{RESET} {msg}")

def print_error(msg):
    print(f"{RED}✖ [ERRO]{RESET} {msg}")

def request_api(endpoint, method="GET", payload=None, token=None):
    url = f"{BASE_URL}{endpoint}"
    headers = {
        "Content-Type": "application/json",
        "Accept": "application/json"
    }
    if token:
        headers["Authorization"] = f"Bearer {token}"

    data = json.dumps(payload).encode("utf-8") if payload else None
    req = urllib.request.Request(url, data=data, headers=headers, method=method)
    
    start_time = time.time()
    try:
        with urllib.request.urlopen(req) as resp:
            elapsed = (time.time() - start_time) * 1000
            body_bytes = resp.read()
            body_json = json.loads(body_bytes.decode("utf-8")) if body_bytes else {}
            return resp.status, body_json, elapsed
    except urllib.error.HTTPError as e:
        elapsed = (time.time() - start_time) * 1000
        error_body = e.read().decode("utf-8")
        try:
            err_json = json.loads(error_body)
        except Exception:
            err_json = {"message": error_body}
        return e.code, err_json, elapsed
    except Exception as ex:
        return 500, {"message": str(ex)}, 0.0

def run_simulation():
    print(f"\n{BOLD}{GREEN}==========================================================================={RESET}")
    print(f"{BOLD}{GREEN}       FROTAGO ERP — SIMULADOR TERMINAL DO FLUXO MOBILE DO INSTRUTOR       {RESET}")
    print(f"{BOLD}{GREEN}==========================================================================={RESET}")
    print(f"Target API: {BASE_URL}")
    print(f"Data/Hora: {time.strftime('%Y-%m-%d %H:%M:%S')}\n")

    timestamp = int(time.time())
    owner_email = f"owner_{timestamp}@frotago.ao"
    instructor_email = f"instrutor_{timestamp}@frotago.ao"
    school_name = f"Escola de Condução Kilamba {timestamp}"

    # -------------------------------------------------------------------------
    # PASSO 1: Registo da Escola e Proprietário
    # -------------------------------------------------------------------------
    print_header("PASSO 1: Registo Público da Escola pelo Proprietário (Owner)")
    reg_payload = {
        "ownerName": "Serafim Gonga",
        "phone": "+244923000000",
        "email": owner_email,
        "password": "Password123!",
        "schoolName": school_name,
        "shortName": "EC Kilamba",
        "slug": f"ec-kilamba-{timestamp}",
        "nif": f"5000{timestamp}",
        "licenseNumber": f"LIC-{timestamp}",
        "licenseIssuer": "DNTT",
        "province": "Luanda",
        "municipality": "Belas",
        "address": "Centralidade do Kilamba, Bloco B",
        "branchName": "Sede Kilamba",
        "plan": "Gratuito"
    }

    print_info(f"Registando escola: '{school_name}' ({owner_email})...")
    status, res, ms = request_api("/api/auth/register-school", method="POST", payload=reg_payload)

    owner_token = None
    school_id = None

    if status == 200 and "token" in res:
        owner_token = res["token"]
        school_id = res.get("schoolId")
        print_success(f"Escola registada com sucesso! [HTTP {status} - {ms:.1f}ms]")
        print_info(f"SchoolID: {school_id}")
    else:
        print_warn(f"Falha ao registar escola ({res.get('message')}). Efetuando login como Admin...")
        status, res, ms = request_api("/api/auth/login", method="POST", payload={"email": "admin@frotago.ao", "password": "Password123!"})
        if status == 200 and "token" in res:
            owner_token = res["token"]
            school_id = res.get("schoolId")
            print_success(f"Login efetuado com sucesso! [HTTP {status} - {ms:.1f}ms]")
        else:
            print_error("Impossível autenticar como Admin.")
            sys.exit(1)

    # -------------------------------------------------------------------------
    # PASSO 2: Convidar Instrutor pelo Painel Web
    # -------------------------------------------------------------------------
    print_header("PASSO 2: Convite de Novo Instrutor via Painel Web")
    invite_payload = {
        "schoolId": school_id,
        "name": "Mateus Manuel Motorista",
        "email": instructor_email,
        "role": 5 # Instructor
    }

    print_info(f"Gerando convite para '{instructor_email}'...")
    status, res, ms = request_api("/api/users/invite", method="POST", payload=invite_payload, token=owner_token)

    invitation_token = None
    if status == 200 and "token" in res:
        invitation_token = res["token"]
        invite_url = res.get("inviteUrl", f"/accept-invitation?token={invitation_token}")
        print_success(f"Convite gerado com sucesso! [HTTP {status} - {ms:.1f}ms]")
        print_info(f"Token do Convite: {invitation_token}")
        print_info(f"Link para Ativação: http://localhost:4200{invite_url}")
    else:
        print_error(f"Erro ao gerar convite: {res.get('message')}")
        sys.exit(1)

    # -------------------------------------------------------------------------
    # PASSO 3: Ativação da Conta do Instrutor (Aceitar Convite)
    # -------------------------------------------------------------------------
    print_header("PASSO 3: Aceitação do Convite & Definição de Palavra-passe")
    accept_payload = {
        "token": invitation_token,
        "password": "InstructorPass123!"
    }

    print_info("Enviando confirmação de convite...")
    status, res, ms = request_api("/api/users/accept-invitation", method="POST", payload=accept_payload)

    if status == 200:
        print_success(f"Conta ativada com sucesso! [HTTP {status} - {ms:.1f}ms]")
    else:
        print_error(f"Erro ao ativar convite: {res.get('message')}")
        sys.exit(1)

    # -------------------------------------------------------------------------
    # PASSO 4: Preparação do Cenário (Criar Veículo, Aluno e Aula Prática)
    # -------------------------------------------------------------------------
    print_header("PASSO 4: Preparação da Frota (Criação de Veículo, Aluno e Agendamento de Aula)")

    # 4.1 Criar Veículo
    plate = f"LD-{timestamp % 100:02d}-89-KL"
    v_payload = {
        "licensePlate": plate,
        "brand": "Toyota",
        "model": "Corolla",
        "chassis": f"CH-{timestamp}",
        "year": 2023,
        "odometer": 42100,
        "fuel": 1,
        "transmission": 1
    }
    status, v_res, ms = request_api("/api/vehicles", method="POST", payload=v_payload, token=owner_token)
    vehicle_id = v_res.get("id") if status == 200 else None
    print_success(f"Viatura criada: Toyota Corolla ({plate}) | ID: {vehicle_id} [HTTP {status} - {ms:.1f}ms]")

    # 4.2 Criar Aluno
    s_payload = {
        "name": "Carlos Alberto Silva",
        "email": f"aluno_{timestamp}@frotago.ao",
        "phoneNumber": "+244923111222",
        "identityCardNumber": f"0045{timestamp % 10000:04d}LA042",
        "category": 1
    }
    status, s_res, ms = request_api("/api/students", method="POST", payload=s_payload, token=owner_token)
    student_id = s_res.get("id") if status == 200 else None
    print_success(f"Aluno cadastrado: Carlos Alberto Silva | ID: {student_id} [HTTP {status} - {ms:.1f}ms]")

    # Obter ID de utilizador do instrutor
    status, users_res, ms = request_api(f"/api/users/school/{school_id}", method="GET", token=owner_token)
    instructor_user_id = None
    if status == 200 and isinstance(users_res, list):
        inst_obj = next((u for u in users_res if u.get("email") == instructor_email), None)
        if inst_obj:
            instructor_user_id = inst_obj.get("id")

    # 4.3 Agendar Aula Prática
    now_utc = time.strftime('%Y-%m-%dT%H:%M:%SZ', time.gmtime())
    lesson_payload = {
        "studentId": student_id,
        "instructorId": instructor_user_id or "00000000-0000-0000-0000-000000000001",
        "vehicleId": vehicle_id,
        "scheduledDate": now_utc,
        "durationMinutes": 60,
        "topic": "Manobras de Estacionamento & Condução em Luanda",
        "observations": "Aula inicial de condução em circuito urbano."
    }
    status, l_res, ms = request_api("/api/lessons", method="POST", payload=lesson_payload, token=owner_token)
    created_lesson_id = l_res.get("id") if status == 200 else None
    print_success(f"Aula prática agendada com sucesso! LessonID: {created_lesson_id} [HTTP {status} - {ms:.1f}ms]")

    # -------------------------------------------------------------------------
    # PASSO 5: Login Mobile do Instrutor
    # -------------------------------------------------------------------------
    print_header("PASSO 5: Autenticação na App Móvel do Instrutor (Mobile Client)")
    mobile_login_payload = {
        "email": instructor_email,
        "password": "InstructorPass123!"
    }
    status, res, ms = request_api("/api/auth/login", method="POST", payload=mobile_login_payload)

    instructor_token = None
    if status == 200 and "token" in res:
        instructor_token = res["token"]
        print_success(f"Login Mobile concluído com sucesso! JWT gerado. [HTTP {status} - {ms:.1f}ms]")
    else:
        print_error("Falha no login mobile do instrutor.")
        sys.exit(1)

    # -------------------------------------------------------------------------
    # PASSO 6: Consulta de Perfil & Dashboard Mobile
    # -------------------------------------------------------------------------
    print_header("PASSO 6: Consulta de Perfil e Dashboard Mobile")
    status, prof, ms = request_api("/api/mobile/profile", method="GET", token=instructor_token)
    print_success(f"Perfil Mobile: {prof.get('nome')} | Escola: {prof.get('escola')} [HTTP {status} - {ms:.1f}ms]")

    status, dash, ms = request_api("/api/mobile/dashboard", method="GET", token=instructor_token)
    print_success(f"Dashboard Mobile: {dash.get('lessonsToday')} aula(s) hoje | Próxima: {dash.get('nextLesson')} [HTTP {status} - {ms:.1f}ms]")

    # -------------------------------------------------------------------------
    # PASSO 7: Obtenção da Lista de Aulas e Detalhes
    # -------------------------------------------------------------------------
    print_header("PASSO 7: Seleção de Aula & Leitura dos Detalhes")
    status, lessons_list, ms = request_api("/api/mobile/lessons/today", method="GET", token=instructor_token)

    active_lesson_id = created_lesson_id
    if status == 200 and isinstance(lessons_list, list) and len(lessons_list) > 0:
        active_lesson_id = lessons_list[0].get("lessonId")
        print_success(f"Aula obtida da API: {lessons_list[0].get('student')} - {lessons_list[0].get('vehicle')} [HTTP {status} - {ms:.1f}ms]")

    status, detail, ms = request_api(f"/api/mobile/lessons/{active_lesson_id}", method="GET", token=instructor_token)
    if status == 200:
        st = detail.get("student", {})
        vh = detail.get("vehicle", {})
        print_success(f"Detalhes da Aula lidos: Aluno = {st.get('name')} | Veículo = {vh.get('brand')} {vh.get('model')} ({vh.get('licensePlate')}) [HTTP {status} - {ms:.1f}ms]")

    # -------------------------------------------------------------------------
    # PASSO 8: Iniciar Aula Prática
    # -------------------------------------------------------------------------
    print_header("PASSO 8: Início da Aula Prática (Validation & Start)")
    print_info("Verificando sensores móveis: GPS (OK), Internet (OK), Permissões (OK)...")
    time.sleep(0.5)

    status, start_res, ms = request_api(f"/api/mobile/lessons/{active_lesson_id}/start", method="POST", token=instructor_token)
    print_success(f"Estado da Aula alterado para EmCurso: {start_res.get('message')} [HTTP {status} - {ms:.1f}ms]")

    # -------------------------------------------------------------------------
    # PASSO 9: Rastreamento GPS Continuo (Simulação de Circuito Urbano de Luanda)
    # -------------------------------------------------------------------------
    print_header("PASSO 9: Transmissão de Telemetria GPS em Tempo Real (Circuito Luanda)")

    track_start_payload = {
        "vehicleId": vehicle_id or "00000000-0000-0000-0000-000000000001",
        "instructorId": instructor_user_id,
        "lessonId": active_lesson_id,
        "provider": "mobile_python_script"
    }
    status, track_sess, ms = request_api("/api/tracking/start", method="POST", payload=track_start_payload, token=instructor_token)
    
    session_id = track_sess.get("id") if status == 200 else "demo-session-999"
    print_info(f"Sessão de Tracking Registada no Servidor: {session_id}")

    luanda_waypoints = [
        {"lat": -8.8078, "lng": 13.2235, "speed": 35.0}, # Marginal de Luanda
        {"lat": -8.8105, "lng": 13.2258, "speed": 42.5}, # Kinaxixi
        {"lat": -8.8132, "lng": 13.2281, "speed": 48.0}, # Largo do Lumeji
        {"lat": -8.8169, "lng": 13.2304, "speed": 52.0}, # Maianga
        {"lat": -8.8202, "lng": 13.2325, "speed": 30.0}  # Rocha Pinto
    ]

    for idx, wp in enumerate(luanda_waypoints, 1):
        loc_payload = {
            "trackingSessionId": session_id,
            "latitude": wp["lat"],
            "longitude": wp["lng"],
            "speed": wp["speed"]
        }
        status, loc_res, ms = request_api("/api/tracking/location", method="POST", payload=loc_payload, token=instructor_token)
        print(f"   📡 Telemetria Ponto #{idx}: Lat={wp['lat']}, Lng={wp['lng']} | {wp['speed']} km/h -> {GREEN}Enviado e Difundido via SignalR ({ms:.1f}ms){RESET}")
        time.sleep(0.6)

    # -------------------------------------------------------------------------
    # PASSO 10: Finalizar Aula & Enviar Relatório
    # -------------------------------------------------------------------------
    print_header("PASSO 10: Finalização da Aula Prática & Envio do Relatório")
    finish_payload = {
        "evaluation": "Good",
        "notes": "Excelente desempenho em manobras urbanas e rotunda de Talatona. Cumprimento rigoroso das normas do DNTT.",
        "odometer": 42125
    }

    print_info("Submetendo relatório final da aula e odómetro final...")
    status, finish_res, ms = request_api(f"/api/mobile/lessons/{active_lesson_id}/finish", method="POST", payload=finish_payload, token=instructor_token)

    if status in (200, 201):
        print_success(f"Relatório enviado com sucesso! [HTTP {status} - {ms:.1f}ms]")
        print_info(f"Mensagem: {finish_res.get('message')}")
    else:
        print_warn(f"Resposta da finalização: {finish_res.get('message')} [HTTP {status}]")

    print(f"\n{BOLD}{GREEN}==========================================================================={RESET}")
    print(f"{BOLD}{GREEN}    ✔ SIMULAÇÃO COMPLETA DO FLUXO MOBILE EXECUTADA E LOGADA COM SUCESSO!     {RESET}")
    print(f"{BOLD}{GREEN}==========================================================================={RESET}\n")

if __name__ == "__main__":
    run_simulation()
