#!/usr/bin/env node
/**
 * Generates manifest.prod.xml from manifest.template.xml by substituting
 * public HTTPS URLs for the add-in static host and API domain.
 *
 * Environment variables:
 *   ADDIN_BASE_URL — origin serving taskpane.html (no trailing slash)
 *   API_BASE_URL   — backend origin for CORS / AppDomains (no trailing slash)
 *   ADDIN_ID       — manifest <Id> GUID (defaults to the prod GUID below)
 *
 * The production manifest deliberately uses a different <Id> than the dev
 * manifests (manifest.dev.xml / manifest.xml keep 92d7d44c-...). Office Online
 * caches add-ins by <Id>; sharing the dev GUID made it serve the cached
 * localhost SourceLocation instead of the freshly uploaded prod manifest.
 */
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const root = path.dirname(fileURLToPath(import.meta.url));
const addInDir = path.resolve(root, "..");

const defaultAddinBase = "https://alexb0nch.github.io/ASN-PP-PowerKeys";
const defaultApiBase = "https://95.140.152.103.sslip.io";
// Production-only GUID, distinct from the dev manifests' 92d7d44c-... Id.
const defaultAddinId = "5b0ca36f-a511-4705-a5e2-9609ff931f85";

const GUID_RE = /^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$/;

function normalizeAddinId(value) {
  const id = value.trim();
  if (!GUID_RE.test(id)) {
    console.error(`ADDIN_ID is not a valid GUID: ${value}`);
    process.exit(1);
  }
  return id;
}

function parseHttpsUrl(url, label) {
  let parsed;
  try {
    parsed = new URL(url);
  } catch {
    console.error(`${label} is not a valid URL: ${url}`);
    process.exit(1);
  }

  if (parsed.protocol !== "https:") {
    console.error(`${label} must use HTTPS for production manifests: ${url}`);
    process.exit(1);
  }

  if (parsed.hostname === "localhost" || parsed.hostname === "127.0.0.1") {
    console.error(`${label} must not point at localhost for production manifests: ${url}`);
    process.exit(1);
  }

  return parsed;
}

/** Base URL for static add-in assets (origin + optional path, no trailing slash). */
function normalizeAddinBase(url) {
  const parsed = parseHttpsUrl(url, "ADDIN_BASE_URL");
  let base = parsed.origin;
  if (parsed.pathname && parsed.pathname !== "/") {
    base += parsed.pathname.replace(/\/$/, "");
  }
  return base;
}

/** API origin for AppDomains / CORS (origin only, no path). */
function normalizeApiOrigin(url) {
  return parseHttpsUrl(url, "API_BASE_URL").origin;
}

/** Page origin for AppDomains (no path — MS manifest requirement). */
function normalizeAddinOrigin(url) {
  return parseHttpsUrl(url, "ADDIN_BASE_URL").origin;
}

const addinBase = normalizeAddinBase(process.env.ADDIN_BASE_URL ?? defaultAddinBase);
const addinOrigin = normalizeAddinOrigin(process.env.ADDIN_BASE_URL ?? defaultAddinBase);
const apiBase = normalizeApiOrigin(process.env.API_BASE_URL ?? defaultApiBase);
const addinId = normalizeAddinId(process.env.ADDIN_ID ?? defaultAddinId);

const templatePath = path.join(addInDir, "manifest.template.xml");
const outputPath = path.join(addInDir, "manifest.prod.xml");

const template = fs.readFileSync(templatePath, "utf8");
const manifest = template
  .replaceAll("{{ADDIN_ID}}", addinId)
  .replaceAll("{{ADDIN_BASE_URL}}", addinBase)
  .replaceAll("{{ADDIN_ORIGIN}}", addinOrigin)
  .replaceAll("{{API_DOMAIN}}", apiBase);

fs.writeFileSync(outputPath, manifest, "utf8");
console.log(`Wrote ${outputPath}`);
console.log(`  ADDIN_ID       = ${addinId}`);
console.log(`  ADDIN_BASE_URL = ${addinBase}`);
console.log(`  API_BASE_URL   = ${apiBase}`);
