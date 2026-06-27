#!/usr/bin/env node
/**
 * Generates manifest.prod.xml from manifest.template.xml by substituting
 * public HTTPS URLs for the add-in static host and API domain.
 *
 * Environment variables:
 *   ADDIN_BASE_URL — origin serving taskpane.html (no trailing slash)
 *   API_BASE_URL   — backend origin for CORS / AppDomains (no trailing slash)
 */
import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

const root = path.dirname(fileURLToPath(import.meta.url));
const addInDir = path.resolve(root, "..");

const defaultAddinBase = "https://alexbonch.github.io/ASN-PP-PowerKeys";
const defaultApiBase = "https://pptpowerkeys-api.azurewebsites.net";

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

/** API origin for AppDomains / CORS (origin only). */
function normalizeApiOrigin(url) {
  return parseHttpsUrl(url, "API_BASE_URL").origin;
}

const addinBase = normalizeAddinBase(process.env.ADDIN_BASE_URL ?? defaultAddinBase);
const apiBase = normalizeApiOrigin(process.env.API_BASE_URL ?? defaultApiBase);

const templatePath = path.join(addInDir, "manifest.template.xml");
const outputPath = path.join(addInDir, "manifest.prod.xml");

const template = fs.readFileSync(templatePath, "utf8");
const manifest = template
  .replaceAll("{{ADDIN_BASE_URL}}", addinBase)
  .replaceAll("{{API_DOMAIN}}", apiBase);

fs.writeFileSync(outputPath, manifest, "utf8");
console.log(`Wrote ${outputPath}`);
console.log(`  ADDIN_BASE_URL = ${addinBase}`);
console.log(`  API_BASE_URL   = ${apiBase}`);
