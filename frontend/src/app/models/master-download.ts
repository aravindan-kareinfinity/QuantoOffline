/** Mirrors InfyPOS.Processors.OfflineClient WindowsOffline* DTOs. */

export type MasterSourceMode = 'Server' | 'Client';

export class OfflineMasterSettings {
  masterSource: MasterSourceMode = 'Server';
  /** Matches backend App.config ServerURL when empty localStorage. */
  serverUrl = 'https://demo.quantosaas.com';
  clientUrl = '';
  organizationCode = '';
  /** Matches backend App.config Locationcode (branch / store code). */
  locationCode = 'MAIN';
  /**
   * Matches backend App.config Data — folder for master.data / stock.data.
   * Browser cannot write here directly; files are saved/downloaded for this folder.
   */
  dataPath = 'D:\\aravindan\\project\\shopppingkart';
  lastDownloadedAt: string | null = null;
}

export class WindowsOfflineRequest {
  systemkey = '';
  locationcode = '';
  autobarcode = false;
  username = '';
  password = '';
  datafrom: string | null = null;
  zerostock = false;
  zerostockfrom: string | null = null;
  datato: string | null = null;
  datatype = 'master';
  data: string | null = null;
  uploadon: string | null = null;
  machinename = '';
  localipaddress = '';
  internetipaddress = '';
  organizationid = 0;
  userid = 0;
  /** Backend typo — keep as API expects. */
  orgainzationcode = '';
  locationid = 0;
}

export class WindowsOfflineResponse {
  key = '';
  status = '';
  progress = 0;
  error = false;
  errormessage = '';
  completed = false;
  noofrecords = 0;
  /** Base64 payload when present. */
  data: string | null = null;
  completedon: string | null = null;
}

export class ClientMasterPackage {
  error = false;
  errormessage = '';
  completed = false;
  lastSyncOn: string | null = null;
  master: string | null = null;
  stock: string | null = null;
  customer: string | null = null;
}

export class MasterDownloadFormModel {
  serverUrl = 'https://demo.quantosaas.com';
  organizationCode = '';
  organizationEditable = false;
  locationCode = 'MAIN';
  /** Local folder for master.data (same as App.config Data). */
  dataPath = 'D:\\aravindan\\project\\shopppingkart';
  username = '';
  password = '';
  zeroStock = false;
  zeroStockFrom = '';
  masterSource: MasterSourceMode = 'Server';
}
